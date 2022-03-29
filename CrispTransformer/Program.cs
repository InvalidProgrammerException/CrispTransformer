using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrispTransform
{
    class Program
    {
        const string logfileVariable = "logfile";
        const string outputfileVariable = "outputfile";
        const string csvfileVariable = "csvfile";
        const string configfileVariable = "configfile";

        static Dictionary<string, string> ReadArguments(string[] args)
        {
            Dictionary<string, string> argToValueMap = new Dictionary<string, string>
            {
                { logfileVariable, "" },
                { outputfileVariable, "" },
                { csvfileVariable, "" },
                { configfileVariable, "" }
            };

            foreach (var arg in args)
            {
                var splits = arg.Split(new string[] { ":=" }, StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length == 2)
                {
                    if (!argToValueMap.ContainsKey(splits[0]))
                    {
                        Console.WriteLine("Expected argument to be of form logfile:=PathToLogFile or outputfile:=PathToOutputFile or csvfile:=PathToCsvFile");
                        return null;
                    }
                    else
                    {
                        argToValueMap[splits[0]] = splits[1];
                    }
                }
                else
                {
                    Console.WriteLine("Expected argument to be of form logfile:=PathToLogFile or outputfile:=PathToOutputFile or csvfile:=PathToCsvFile");
                    return null;
                }
            }

            foreach (var pair in argToValueMap)
            {
                if (string.IsNullOrWhiteSpace(pair.Value))
                {
                    Console.WriteLine($"Missing value for variable '{pair.Key}'");
                    return null;
                }
            }

            return argToValueMap;
        }

        static void TransformCsvFile(Dictionary<string, string> argToValueMap)
        {
            //we should check for the existence of the files

            var logStream = new StreamWriter(argToValueMap[logfileVariable]);
            var outStream = new StreamWriter(argToValueMap[outputfileVariable]);
            var csvParser = new TextFieldParser(argToValueMap[csvfileVariable]);
            var configStream = new StreamReader(argToValueMap[configfileVariable]);

            Transform transform = new Transform(csvParser, logStream, outStream, configStream);
            if (!transform.TransformCsv())
            {
                Console.WriteLine("There were errors transforming the csv file - see the log file for details");
            }
        }


        static void Main(string[] args)
        {
            Dictionary<string, string> argToValueMap = ReadArguments(args);
            if (argToValueMap != null)
                TransformCsvFile(argToValueMap);

            //Console.ReadKey();
        }
    }
}
