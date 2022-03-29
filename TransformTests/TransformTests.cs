using CrispTransform;
using NUnit.Framework;
using Moq;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System;

namespace TransformTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        /// <summary>
        /// Verifies the tCompileConfigFile method is called from the constructor
        /// </summary>
        [Test]
        public void TestCompileConfigFileIsCalledFromConstructor()
        {
            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Mock<Transform> transformMock = new Mock<Transform>(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);
            transformMock.Setup(mock => mock.CompileConfigFile()).Returns(true);
            var o = transformMock.Object; //ensure the Tranform constructor is called
            transformMock.Verify(mock => mock.CompileConfigFile(), Times.Once);
        }

        /// <summary>
        /// Verifies the Output method writes the expected string
        /// </summary>
        [Test]
        public void OutputTest()
        {
            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            string testString = "test string";
            transform.Output(testString);
            outputMock.Verify(mock => mock.Write($"{testString},"), Times.Once);
        }

        /// <summary>
        /// Verifies the OutputInt method writes the expected string
        /// </summary>
        [Test]
        public void OuputIntTest()
        {
            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            int testInt = 321;
            transform.OutputInt(testInt);
            outputMock.Verify(mock => mock.Write($"{testInt},"), Times.Once);
        }

        /// <summary>
        /// Verifies the OutputDec method writes the expected string
        /// </summary>
        [Test]
        public void OutputDecTest()
        {
            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            decimal testDec = 123.45m;
            transform.OutputDec(testDec);
            outputMock.Verify(mock => mock.Write($"{testDec},"), Times.Once);
        }

        // <summary>
        /// Verifies the LogLine method writes the expected string
        /// </summary>
        [Test]
        public void LogLineTest()
        {
            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            string test = "log test";
            transform.LogLine(test);
            logMock.Verify(mock => mock.WriteLine($"{test}"), Times.Once);
        }

        /// <summary>
        /// Tests the StrToDec method
        /// </summary>
        [Test]
        public void StrToDecTest()
        {
            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            decimal value;
            Assert.IsTrue(transform.StrToDec("123", out value));
            Assert.AreEqual(123m, value);

            Assert.IsTrue(transform.StrToDec("-45.68", out value));
            Assert.AreEqual(-45.68m, value);

            Assert.IsTrue(transform.StrToDec("0.168", out value));
            Assert.AreEqual(0.168m, value);

            Assert.IsTrue(transform.StrToDec(".321", out value));
            Assert.AreEqual(0.321m, value);

            Assert.IsTrue(transform.StrToDec("w.321", out value));
            Assert.AreEqual(0.321m, value);

            Assert.IsFalse(transform.StrToDec("-.321-", out value));

            Assert.IsFalse(transform.StrToDec("three", out value));
        }

        /// <summary>
        /// Tests the StrToInt method
        /// </summary>
        [Test]
        public void StrToIntTest()
        {

            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            int value;
            Assert.IsTrue(transform.StrToInt("123", out value));
            Assert.AreEqual(123, value);

            Assert.IsFalse(transform.StrToInt("-45.68", out value));

            Assert.IsFalse(transform.StrToInt("0.168", out value));

            Assert.IsFalse(transform.StrToInt("321.0", out value));

            Assert.IsTrue(transform.StrToInt("w321", out value));
            Assert.AreEqual(321, value);

            Assert.IsFalse(transform.StrToInt("-321-", out value));

            Assert.IsFalse(transform.StrToInt("three", out value));

            Assert.IsTrue(transform.StrToInt("-4321", out value));
            Assert.AreEqual(-4321, value);
        }

        /// <summary>
        /// Tests the ReadCsvHeaderFields method
        /// </summary>
        [Test]
        public void ReadCsvHeaderFieldsTest()
        {
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            writer.WriteLine("a,b,c,d,e");
            writer.Flush();
            csvStream.Position = 0;

            var csvParserMock = new Mock<TextFieldParser>(csvStream);
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            Assert.AreEqual(1, transform._currentLineOfCsvFile);
            Assert.IsTrue(transform.ReadCsvHeaderFields());
            Assert.AreEqual(2, transform._currentLineOfCsvFile);
            Assert.AreEqual(5, transform._expectedFieldsForCsvFile.Count);
            Assert.IsTrue(transform._expectedFieldsForCsvFile.Contains("a"));
            Assert.IsTrue(transform._expectedFieldsForCsvFile.Contains("b"));
            Assert.IsTrue(transform._expectedFieldsForCsvFile.Contains("c"));
            Assert.IsTrue(transform._expectedFieldsForCsvFile.Contains("d"));
            Assert.IsTrue(transform._expectedFieldsForCsvFile.Contains("e"));
        }


        /// <summary>
        /// Tests the ReadAndHandleOneInputCsvLine method
        /// </summary>
        [Test]
        public void ReadAndHandleOneInputCsvLineTest()
        {
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            writer.WriteLine("a,b,c,d,e");
            writer.WriteLine("hello everyone, world, 123, 45.6, 12w");
            writer.Flush();
            csvStream.Position = 0;

            var csvParserMock = new Mock<TextFieldParser>(csvStream);
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            Assert.AreEqual(1, transform._currentLineOfCsvFile);
            Assert.IsTrue(transform.ReadCsvHeaderFields());
            Assert.AreEqual(2, transform._currentLineOfCsvFile);
            transform.ReadAndHandleOneInputCsvLine();
            Assert.AreEqual(3, transform._currentLineOfCsvFile);
            Assert.AreEqual("hello everyone", transform._fieldsForCurrentRowOfCsvFile["a"]);
            Assert.AreEqual("world", transform._fieldsForCurrentRowOfCsvFile["b"]);
            Assert.AreEqual("123", transform._fieldsForCurrentRowOfCsvFile["c"]);
            Assert.AreEqual("45.6", transform._fieldsForCurrentRowOfCsvFile["d"]);
            Assert.AreEqual("12w", transform._fieldsForCurrentRowOfCsvFile["e"]);
        }


        /// <summary>
        /// Tests the TransformCsv method
        /// </summary>
        [Test]
        public void TransformCsvTest()
        {
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            writer.WriteLine("a,b,c,d,e");
            writer.WriteLine("hello everyone, world, 123, 45.6, 12w");
            writer.WriteLine("bye, last, 0, 0.1, 1m");
            writer.Flush();
            csvStream.Position = 0;

            var csvParserMock = new Mock<TextFieldParser>(csvStream);
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Mock<Transform> transformMock = new Mock<Transform>(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);
            transformMock.CallBase = true;
            var transform = transformMock.Object;

            Assert.AreEqual(1, transform._currentLineOfCsvFile);
            Assert.IsTrue(transform.TransformCsv());
            transformMock.Verify(mock => mock.ReadCsvHeaderFields(), Times.Once);
            transformMock.Verify(mock => mock.ReadAndHandleOneInputCsvLine(), Times.Exactly(2));

            Assert.AreEqual(4, transform._currentLineOfCsvFile);
        }


        /// <summary>
        /// Test the Field method
        /// </summary>
        [Test]
        public void FieldTest()
        {
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            writer.WriteLine("a,b,c,d,e");
            writer.WriteLine("hello everyone, world, 123, 45.6, 12w");
            writer.Flush();
            csvStream.Position = 0;

            var csvParserMock = new Mock<TextFieldParser>(csvStream);
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            transform.ReadCsvHeaderFields();
            transform.ReadAndHandleOneInputCsvLine();
            Assert.AreEqual("hello everyone", transform.Field("a"));
            Assert.AreEqual("world", transform.Field("b"));
            Assert.AreEqual("123", transform.Field("c"));
            Assert.AreEqual("45.6", transform.Field("d"));
            Assert.AreEqual("12w", transform.Field("e"));
        }

        /// <summary>
        /// Tests the ValidateValueMethod
        /// </summary>
        [Test]
        public void ValidateValueTest()
        {
            var csvParserMock = new Mock<TextFieldParser>(new MemoryStream());
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Transform transform = new Transform(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);

            Assert.IsTrue(transform.ValidateValue("321.45", "\\d+[.]\\d+"));
            Assert.IsTrue(transform.ValidateValue("w321.45", "\\d+[.]\\d+"));
            Assert.IsFalse(transform.ValidateValue("w321.45", "^\\d+[.]\\d+"));
        }

        /// <summary>
        /// Tests the ValidateFieldMethod
        /// </summary>
        [Test]
        public void ValidateFieldTest()
        {
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            writer.WriteLine("a");
            writer.WriteLine("w321.45");
            writer.Flush();
            csvStream.Position = 0;

            var csvParserMock = new Mock<TextFieldParser>(csvStream);
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(new MemoryStream());
            var configMock = new Mock<StreamReader>(new MemoryStream());
            Mock<Transform> transformMock = new Mock<Transform>(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);
            transformMock.CallBase = true;
            var transform = transformMock.Object;

            transform.ReadCsvHeaderFields();
            transform.ReadAndHandleOneInputCsvLine();
            Assert.IsFalse(transform.ValidateField("b", "\\d+[.]\\d+"));
            transformMock.Verify(mock => mock.LogLine($"When calling {nameof(Transform.ValidateField)}, the field name 'b' is invalid."));

            Assert.IsTrue(transform.ValidateField("a", "\\d+[.]\\d+"));
            Assert.IsFalse(transform.ValidateField("a", "^\\d+[.]\\d+"));
        }

        /// <summary>
        /// Tests the CompileConfigFile and that code from it runs successfully
        /// </summary>
        [Test]
        public void CompileConfigFileSucceedsAndCodeRuns()
        {
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            writer.WriteLine("a");
            writer.WriteLine("321.45");
            writer.Flush();
            csvStream.Position = 0;

            var configStream = new MemoryStream();
            writer = new StreamWriter(configStream);
            writer.WriteLine("decimal d; transform.StrToDec(transform.Field(\"a\"), out d); transform.OutputDec(d + 1.0m);");
            writer.Flush();
            configStream.Position = 0;

            var outputStream = new MemoryStream();

            var csvParserMock = new Mock<TextFieldParser>(csvStream);
            csvParserMock.CallBase = true;
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(outputStream);
            outputMock.CallBase = true;
            var configMock = new Mock<StreamReader>(configStream);
            configMock.CallBase = true;
            Mock<Transform> transformMock = new Mock<Transform>(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);
            transformMock.CallBase = true;
            var transform = transformMock.Object;

            Assert.IsTrue(transform.TransformCsv());

            outputStream.Position = 0;
            StreamReader sr = new StreamReader(outputStream);
            string output = sr.ReadLine();
            Assert.AreEqual("322.45,", output);
        }

        /// <summary>
        /// Tests the CompileConfigFile fails with invalid code
        /// </summary>
        [Test]
        public void CompileConfigFileFailsWithInvalidCode()
        {
            var csvStream = new MemoryStream();
            var writer = new StreamWriter(csvStream);
            writer.WriteLine("a");
            writer.WriteLine("321.45");
            writer.Flush();
            csvStream.Position = 0;

            var configStream = new MemoryStream();
            writer = new StreamWriter(configStream);
            writer.WriteLine("decimal d");
            writer.Flush();
            configStream.Position = 0;

            var outputStream = new MemoryStream();

            var csvParserMock = new Mock<TextFieldParser>(csvStream);
            csvParserMock.CallBase = true;
            var logMock = new Mock<StreamWriter>(new MemoryStream());
            var outputMock = new Mock<StreamWriter>(outputStream);
            outputMock.CallBase = true;
            var configMock = new Mock<StreamReader>(configStream);
            configMock.CallBase = true;
            Mock<Transform> transformMock = new Mock<Transform>(csvParserMock.Object, logMock.Object,
                outputMock.Object, configMock.Object);
            transformMock.CallBase = true;
            
            var ex = Assert.Throws<System.Reflection.TargetInvocationException>(() => { var transform = transformMock.Object; });
            Assert.AreEqual(typeof(InvalidProgramException), ex.InnerException.GetType());

            transformMock.Verify(mock => mock.LogLine("There were errors compiling the config file:"));
        }

    }
}