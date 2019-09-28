using System.IO;
using System.Collections.Generic;
using CPTLib.LanguageHandlers;
using CPTLib.Models;
using CPTLib.Models.Enums;

namespace CPTLib
{
    public static class CheckHandler
    {
        public static TestResult Check(string solutionCodeFilePath, string testInputFilePath, string testOutputFilePath, 
            int timeLimit, int memoryLimit, ref double usedTime, ref double usedMemory, LanguageHandler languageHandler)
        {
            string output = "";
            string tempErrors = "";

            var compilationAndExecutionResult = CompileAndExecute(solutionCodeFilePath, timeLimit, memoryLimit,
                languageHandler, testInputFilePath, ref output, ref tempErrors, ref usedTime, ref usedMemory, false);
            if (compilationAndExecutionResult == ResultMessage.CE || compilationAndExecutionResult == ResultMessage.EE)
            {
                return new TestResult(tempErrors, compilationAndExecutionResult, usedTime, usedMemory);
            }

            var sr = new StreamReader(testOutputFilePath);
            var testOutput = sr.ReadToEnd();
            sr.Close();

            if (string.Compare(output, testOutput, true) != 0)
            {
                return new TestResult("Expected:\n" + testOutput + "\n\n" + "Actual:\n" + output, ResultMessage.TE, usedTime, usedMemory);
            }

            return new TestResult("", ResultMessage.OK, usedTime, usedMemory);
        }

        public static TestResult CheckWithChecker(string solutionCodePath, string checkerCodePath, string testInputFilePath, 
            string testOutputFilePath, int timeLimit, int memoryLimit, ref double usedTime, ref double usedMemory, LanguageHandler languageHandler)
        {
            string output = "";
            string tempErrors = "";

            var compilationAndExecutionResult = CompileAndExecute(solutionCodePath, timeLimit, memoryLimit,
                languageHandler, testInputFilePath, ref output, ref tempErrors, ref usedTime, ref usedMemory, false);
            if (compilationAndExecutionResult == ResultMessage.CE || compilationAndExecutionResult == ResultMessage.EE)
            {
                return new TestResult("solution:\r\n" + tempErrors, compilationAndExecutionResult, usedTime, usedMemory);
            }
            
            var outputFilePath = Path.GetDirectoryName(solutionCodePath) + @"\output.txt";

            var sw = new StreamWriter(outputFilePath);
            sw.Write(output);
            sw.Close();

            string checkerOutput = "";
            var checkerCompilationAndExecutionResult = CompileAndExecute(checkerCodePath, timeLimit, memoryLimit, languageHandler,
                outputFilePath, ref checkerOutput, ref tempErrors, ref usedTime, ref usedMemory, true, testInputFilePath, testOutputFilePath);
            if (checkerCompilationAndExecutionResult == ResultMessage.CE || 
                checkerCompilationAndExecutionResult == ResultMessage.EE)
            {
                return new TestResult("checker:\r\n" + tempErrors, checkerCompilationAndExecutionResult, usedTime, usedMemory);
            }

            var sr = new StreamReader(testOutputFilePath);
            var testOutput = sr.ReadToEnd();
            sr.Close();

            //checker should return "true" or "false"
            if (checkerOutput.ToLower().Replace("\r\n", "") == "true")
            {
                return new TestResult("", ResultMessage.OK, usedTime, usedMemory);
            }
            return new TestResult("Expected:\n" + testOutput + "\n\n" + "Actual:\n" + output, ResultMessage.TE, usedTime, usedMemory);
        }

        private static ResultMessage CompileAndExecute(string solutionCodePath, int timeLimit, int memoryLimit, LanguageHandler languageHandler, 
            string inputFilePath, ref string output, ref string errors, ref double usedTime, ref double usedMemory, bool isChecker, string inputTestFilePath = "", string outputTestFilePath = "")
        {
            var compileResult = languageHandler.Compile(solutionCodePath, ref errors);
            if (!compileResult)
            {;
                return ResultMessage.CE;
            }
            
            var extension = Path.GetExtension(solutionCodePath);
            var executableFilePath = solutionCodePath.Substring(0, solutionCodePath.Length - extension.Length) + ".exe";
            bool executeResult = false;
            if (isChecker)
            {
                executeResult = languageHandler.Execute(executableFilePath, inputFilePath, ref output, ref errors,
                    timeLimit, memoryLimit, ref usedTime, ref usedMemory, true, inputTestFilePath, outputTestFilePath);
            }
            else
            {
                executeResult = languageHandler.Execute(executableFilePath, inputFilePath, ref output, ref errors, timeLimit,
                    memoryLimit, ref usedTime, ref usedMemory, false);
            }
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
