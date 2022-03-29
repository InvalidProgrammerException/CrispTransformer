using Microsoft.CSharp;
using Microsoft.VisualBasic.FileIO;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CrispTransform
{
    /// <summary>
    /// Class for transforming an input csv to an output csv
    /// </summary>
    public class Transform
    {
        //The parser for the input csv file
        TextFieldParser _csvParser;
        //The log stream
        StreamWriter _logStream;
        //The csv output stream
        StreamWriter _outStream;
        //The conifguration file stream
        StreamReader _configFile;
        //HashSet containing the column headers we read from the first row of the csv input file
        internal HashSet<string> _expectedFieldsForCsvFile = new HashSet<string>();
        //Maps the field header index to the field header name
        Dictionary<int, string> _indexToFieldHeader = new Dictionary<int, string>();
        //Map of field name to field value for current row of input csv file
        internal Dictionary<string, string> _fieldsForCurrentRowOfCsvFile = new Dictionary<string, string>();
        //The line of the input csv we are currently positioned on> internal for testing purposes
        internal int _currentLineOfCsvFile = 1;
        //The object for processing rows
        dynamic _rowProcessor;

        const string StringToReplaceWithUserDefinedCode = "#user_defined_function#";

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="csvParser">The parser for the input csv file</param>
        /// <param name="logStream">The log stream</param>
        /// <param name="outStream">The csv output stream</param>
        /// <param name="configFile">The conifguration file stream</param>
        public Transform(TextFieldParser csvParser, StreamWriter logStream, StreamWriter outStream, StreamReader configFile)
        {
            _csvParser = csvParser;
            _csvParser.SetDelimiters(",");
            _logStream = logStream;
            _logStream.AutoFlush = true;
            _outStream = outStream;
            _outStream.AutoFlush = true;
            _configFile = configFile;

            if (!CompileConfigFile())
                throw new InvalidProgramException($"There were errors compiling the config file");
        }

        /// <summary>
        /// Compiles the configuration file and logs any compile errors (if any). Internal for testing purposes only
        /// </summary>
        /// <returns>true, if the confile file compiled successfully, else false</returns>
        virtual internal bool CompileConfigFile()
        {
            try
            {
                string userDefinedFunctionality = _configFile.ReadToEnd();
                string rowProcessorCode = InitialRowProcessorCode.Replace(StringToReplaceWithUserDefinedCode, userDefinedFunctionality);

                CSharpCodeProvider provider = new CSharpCodeProvider();
                CompilerParameters parameters = new CompilerParameters();
                var thisAssembly = Assembly.GetExecutingAssembly();
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Data.dll");
                parameters.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
                parameters.ReferencedAssemblies.Add(thisAssembly.Location);
                CompilerResults results = provider.CompileAssemblyFromSource(parameters, rowProcessorCode);
                var compilerErrors = results.Errors.Cast<CompilerError>().ToList();
                if (compilerErrors.Any())
                {
                    LogLine($"There were errors compiling the config file:");
                    string[] codeLines = rowProcessorCode.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var error in compilerErrors)
                    {
                        LogLine($"{error.ErrorText} for line '{codeLines[error.Line - 1].Replace("\r", "")}'");
                    }

                    return false;
                }

                Type rowProcessorType = results.CompiledAssembly.GetType("CrispTransform.RowProcessor");
                var rowProcessorConstructor = rowProcessorType.GetConstructor(new Type[] { typeof(Transform) });
                _rowProcessor = rowProcessorConstructor.Invoke(new object[] { this });
            }
            catch (Exception ex)
            {
                LogLine($"There was an exception '{ex.Message}' when compiling the config file.");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Read the csv header fields. This is expected to be run only on the first line of the csv file. Internal for testing purposes only
        /// </summary>
        /// <returns>ture, if there were no errors, else false</returns>
        virtual internal bool ReadCsvHeaderFields()
        {
            if (_currentLineOfCsvFile != 1)
            {
                throw new ArgumentOutOfRangeException("Attempting to read field headers past the first line of the csv file");
            }

            bool hasErrors = false;
            try
            {
                string[] fieldNames = _csvParser.ReadFields();
                int index = 0;
                foreach (var fieldName in fieldNames)
                {
                    if (_expectedFieldsForCsvFile.Contains(fieldName))
                    {
                        LogLine($"ERROR: Repeated column name '{fieldName}' in first row of csv file");
                        hasErrors = true;
                    }
                    _expectedFieldsForCsvFile.Add(fieldName);
                    _indexToFieldHeader.Add(index, fieldName);
                    index++;
                }
            }
            catch (Exception ex)
            {
                LogLine($"Exception '{ex.Message}' when reading column headers of csv file");
                return false;
            }
            _currentLineOfCsvFile++;
            return !hasErrors;
        }

        /// <summary>
        /// Reads one line of input from the csv file and and handles it (potentially writing to the output file).
        /// </summary>
        /// <returns>true, if we should continue to the next line, else false</returns>
        internal virtual bool ReadAndHandleOneInputCsvLine()
        {
            string[] fields = _csvParser.ReadFields();
            if (fields.Length != _expectedFieldsForCsvFile.Count)
            {
                LogLine($"On line {_currentLineOfCsvFile} of the csv file we expected {_expectedFieldsForCsvFile.Count} fields, but had {fields.Length} fields instead");
                return false;
            }

            _fieldsForCurrentRowOfCsvFile.Clear();
            for (int index = 0; index < fields.Length; ++index)
            {
                _fieldsForCurrentRowOfCsvFile.Add(_indexToFieldHeader[index], fields[index]);
            }

            if (!_rowProcessor.ProcessRow(_currentLineOfCsvFile))
                return false;

            _outStream.WriteLine();

            _currentLineOfCsvFile++;

            return true;
        }


        /// <summary>
        /// Transform a csv file to another form of csv file
        /// </summary>
        /// <returns>true, if successful, else false</returns>
        public virtual bool TransformCsv()
        {
            if (!ReadCsvHeaderFields())
                return false;

            try
            {
                while (!_csvParser.EndOfData)
                {
                    if (!ReadAndHandleOneInputCsvLine())
                        return false;
                }
            }
            catch (Exception ex)
            {
                LogLine($"Threw exception '{ex.Message}' while processing csv file. Last line read was {_currentLineOfCsvFile}.");
                return false;
            }


            return true;
        }

        #region Output functions

        /// <summary>
        /// Output string value to output csv file
        /// </summary>
        /// <param name="value">the value to output</param>
        public virtual void Output(string value)
        {
            _outStream.Write($"{value},");
        }

        /// <summary>
        /// Output int value to output csv file
        /// </summary>
        /// <param name="value">the value to output</param>
        public virtual void OutputInt(int value)
        {
            _outStream.Write($"{value},");
        }

        /// <summary>
        /// Output decimal value to output csv file
        /// </summary>
        /// <param name="value">the value to output</param>
        public virtual void OutputDec(decimal value)
        {
            _outStream.Write($"{value},");
        }

        /// <summary>
        /// Write a line of text to the log file
        /// </summary>
        /// <param name="text">The string to write to the log file</param>
        public virtual void LogLine(string text)
        {
            _logStream.WriteLine(text);
        }

        #endregion

        #region Convenience functions - we may want to consider putting these in another class

        /// <summary>
        /// convert a string to decimal - skipping any non-decimal characters
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <param name="value">the decimal we desire</param>
        /// <returns>true, if the conversion was successful, else false</returns>
        public virtual bool StrToDec(string s, out decimal value)
        {
            value = 0;
            StringBuilder sanitized = new StringBuilder();
            foreach (char c in s)
            {
                if (c == '.' || c == '-' || (c >= '0' && c <= '9'))
                    sanitized.Append(c);
            }

            return Decimal.TryParse(sanitized.ToString(), out value);
        }

        /// <summary>
        /// convert a string to integer - skipping any non-integer characters (except '.', which will be treated as an error)
        /// </summary>
        /// <param name="s">The string to convert</param>
        /// <param name="value">the integer we desire</param>
        /// <returns>true, if the conversion was successful, else false</returns>
        public virtual bool StrToInt(string s, out int value)
        {
            value = 0;
            StringBuilder sanitized = new StringBuilder();
            foreach (char c in s)
            {
                if (c == '.' || c == '-' || (c >= '0' && c <= '9'))
                    sanitized.Append(c);
            }

            return Int32.TryParse(sanitized.ToString(), out value);
        }

        /// <summary>
        /// Retrieves the value for a field for the current row of the input csv file
        /// </summary>
        /// <param name="fieldName">the name of a field to get a value for</param>
        /// <returns>the value for the field</returns>
        public virtual string Field(string fieldName)
        {
            if (!_expectedFieldsForCsvFile.Contains(fieldName))
            {
                LogLine($"When calling {nameof(Field)}, the field name '{fieldName}' is invalid.");
                throw new ArgumentException($"Field name {fieldName} not found when calling Field");
            }

            return _fieldsForCurrentRowOfCsvFile[fieldName];
        }

        /// <summary>
        /// Validates a value against the expected regular expression
        /// </summary>
        /// <param name="valueToTest">the value we want to validate</param>
        /// <param name="validationRegExString">The regex string we want to validate against</param>
        /// <returns>true, if the fields matches the regular expression, else false</returns>
        public virtual bool ValidateValue(string valueToTest, string validationRegExString)
        {
            Regex regEx = new Regex(validationRegExString);

            var isMatch = regEx.IsMatch(valueToTest);
            if (!isMatch)
            {
                LogLine($"Validating value '{valueToTest}' against expression '{validationRegExString}' on line {_currentLineOfCsvFile} failed.");
            }

            return isMatch;
        }


        /// <summary>
        /// Validates a field against the expected regular expression
        /// </summary>
        /// <param name="fieldName">the name of the field we want to validate</param>
        /// <param name="validationRegExString">The regex string we want to validate against</param>
        /// <returns>true, if the fields matches the regular expression, else false</returns>
        public virtual bool ValidateField(string fieldName, string validationRegExString)
        {
            Regex regEx = new Regex(validationRegExString);

            if (!_expectedFieldsForCsvFile.Contains(fieldName))
            {
                LogLine($"When calling {nameof(ValidateField)}, the field name '{fieldName}' is invalid.");
                return false;
            }

            var isMatch = regEx.IsMatch(_fieldsForCurrentRowOfCsvFile[fieldName]);
            if (!isMatch)
            {
                LogLine($"Validating field '{fieldName}' against expression '{validationRegExString}' on line {_currentLineOfCsvFile} failed.");
            }

            return isMatch;
        }

        #endregion 


        const string InitialRowProcessorCode =
@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace CrispTransform
{
    public class RowProcessor
    {
        Transform transform;
        public RowProcessor(Transform transform)
        {
            this.transform = transform;
        }

        //lineNumber is the line number of the csv file we are processing
        //return true to indicate we should continue processing on to the next row, else
        //return false if we should stop processing
        public bool ProcessRow(int lineNumber)
        {
            //start of user defined code

            #user_defined_function#

            //end of user defined code
            
            return true;
        }
    }
}";
    }
}
