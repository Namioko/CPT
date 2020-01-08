using System;
using System.IO;
using System.Collections.Generic;
using CPTLib.Models.ContestObjects;
using CPTLib.Models.ContestObjects.CheckParameters;
using CPTLib.Models.Enums;

namespace CPTLib
{
    public static class ContestHandler
    {
        public static TestResult Check(CheckParameters parameters)
        {
            var output = "";
            var tempErrors = "";

            double usedTime = 0;
            double usedMemory = 0;

            var compilationAndExecutionResult = CompileAndExecute(parameters, ref output, ref tempErrors, ref usedTime, ref usedMemory);
            if (compilationAndExecutionResult == ResultMessage.CE || compilationAndExecutionResult == ResultMessage.EE)
            {
                return new TestResult(tempErrors, compilationAndExecutionResult, usedTime, usedMemory);
            }

            var sr = new StreamReader(parameters.OutputTestFileName);
            var testOutput = sr.ReadToEnd();
            sr.Close();

            return string.Compare(output, testOutput, StringComparison.OrdinalIgnoreCase) != 0 
                ? new TestResult("Expected:\n" + testOutput + "\n\n" + "Actual:\n" + output, ResultMessage.TE, usedTime, usedMemory) 
                : new TestResult("", ResultMessage.OK, usedTime, usedMemory);
        }

        public static TestResult CheckWithChecker(CheckParameters parameters, string checkerFileName)
        {
            var output = "";
            var tempErrors = "";

            double usedTime = 0;
            double usedMemory = 0;

            var compilationAndExecutionResult = CompileAndExecute(parameters, ref output, ref tempErrors, ref usedTime, ref usedMemory);
            if (compilationAndExecutionResult == ResultMessage.CE || compilationAndExecutionResult == ResultMessage.EE)
            {
                return new TestResult("solution:\r\n" + tempErrors, compilationAndExecutionResult, usedTime, usedMemory);
            }
            
            var outputFilePath = Path.GetDirectoryName(parameters.FileName) + @"\output.txt";

            var sw = new StreamWriter(outputFilePath);
            sw.Write(output);
            sw.Close();

            var checkerParameters = new CheckParametersForChecker(parameters, checkerFileName, outputFilePath);

            var checkerOutput = "";
            var checkerCompilationAndExecutionResult = CompileAndExecute(checkerParameters, ref checkerOutput, ref tempErrors, ref usedTime, 
                ref usedMemory, true);
            if (checkerCompilationAndExecutionResult == ResultMessage.CE || checkerCompilationAndExecutionResult == ResultMessage.EE)
            {
                return new TestResult("checker:\r\n" + tempErrors, checkerCompilationAndExecutionResult, usedTime, usedMemory);
            }

            var sr = new StreamReader(parameters.OutputTestFileName);
            var testOutput = sr.ReadToEnd();
            sr.Close();

            //checker should return "true" or "false"
            return checkerOutput.ToLower().Replace("\r\n", "") == "true" 
                ? new TestResult("", ResultMessage.OK, usedTime, usedMemory) 
                : new TestResult("Expected:\n" + testOutput + "\n\n" + "Actual:\n" + output, ResultMessage.TE, usedTime, usedMemory);
        }

        public static string GenerateTestOutput(CheckParametersForGenerator parameters)
        {
            var output = "";
            var tempErrors = "";

            double usedTime = 0;
            double usedMemory = 0;

            var compilationAndExecutionResult = CompileAndExecute(parameters, ref output, ref tempErrors, ref usedTime, ref usedMemory);
            if (compilationAndExecutionResult == ResultMessage.CE || compilationAndExecutionResult == ResultMessage.EE)
            {
                return "Errors: " + tempErrors;
            }

            var sw = new StreamWriter(parameters.OutputTestFileName);
            sw.Write(output);
            sw.Close();
            return "";
        }

        private static ResultMessage CompileAndExecute(CheckParameters parameters, ref string output, ref string errors, 
            ref double usedTime, ref double usedMemory, bool isChecker = false)
        {
            var languageHandler = parameters.LanguageHandler;

            var compileResult = languageHandler.Compile(parameters.FileName, ref errors);
            if (!compileResult)
            {
                return ResultMessage.CE;
            }
            
            var executeResult = languageHandler.Execute(parameters, ref output, ref errors, ref usedTime, ref usedMemory, isChecker);
            return !executeResult ? ResultMessage.EE : ResultMessage.OK;
        }

        private static bool ReadFromInputFile(string inputFile, List<string> input)
        {
            var srInput = new StreamReader(inputFile);

            if (!File.Exists(inputFile))
            {
                return false;
            }

            var line = srInput.ReadLine();
            while (line != null)
            {
                input.Add(line);
                line = srInput.ReadLine();
            }

            return true;
        }
    }
}
