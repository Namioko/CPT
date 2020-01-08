using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using CPTLib;
using CPTLib.LanguageHandlers;
using CPTLib.Models.APIComponents;
using CPTLib.Models.ContestObjects;
using CPTLib.Models.ContestObjects.CheckParameters;
using CPTLib.Models.Enums;
using Newtonsoft.Json;

namespace CPT.Controllers
{
    [RoutePrefix("api")]
    public class ContestController : ApiController
    {
        [HttpPost]
        [Route("check")]
        public IHttpActionResult ExecuteSolution([FromBody]InputData input)
        {
            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var extension = GenerateExtensionFromLanguage(input.Language);

            var solutionFileName = CreateFile(input.Solution, root, "solution", extension);
            var checkerFileName = CreateFile(input.Checker, root, "checker", extension);

            var testResults = new List<TestResult>();

            foreach (var currentTestData in input.Tests)
            {
                var currentInputTestFileName = CreateFile(currentTestData.Input, root, "inputtest", extension);
                var currentOutputTestFileName = CreateFile(currentTestData.Output, root, "outputest", extension);

                var currentTestResult = MakeCheck(solutionFileName, checkerFileName, currentInputTestFileName, 
                    currentOutputTestFileName, input.TimeLimit > 0 ? input.TimeLimit : 0, 
                    input.MemoryLimit > 0 ? input.MemoryLimit : 0, input.Language);

                currentTestResult.Number = currentTestData.Number;
                testResults.Add(currentTestResult);

                if (currentTestResult.Shortening == ResultMessage.CE)
                {
                    break;
                }
            }

            DeleteUnnecessaryFiles(root);

            return Ok(new OutputData(testResults.ToArray()));
        }

        private static string GenerateExtensionFromLanguage(Language language)
        {
            return language == Language.CSharp ? "cs" : language == Language.Cpp ? "cpp" : "cs";
        }

        private static string GenerateFileName(string root, string name, string extension)
        {
            return root + @"\" + name + "." + extension;
        }

        private static string CreateFile(byte[] data, string root, string name, string extension)
        {
            var fileName = GenerateFileName(root, name, extension);
            File.WriteAllBytes(fileName, data);
            return fileName;
        }

        private static TestResult MakeCheck(string solutionFileName, string checkerFileName, string inputTestFileName,
                    string outputTestFileName, int timeLimit, int memoryLimit, Language language, bool withChecker = true)
        {
            LanguageHandler currentLanguageHandler;
            switch (language)
            {
                case Language.CSharp:
                    currentLanguageHandler = new CSharpHandler();
                    break;

                case Language.Cpp:
                    currentLanguageHandler = new CppHandler();
                    break;

                default:
                    currentLanguageHandler = new CSharpHandler();
                    break;
            }

            CheckParameters parameters = new CheckParametersForSolution(solutionFileName, inputTestFileName, outputTestFileName, 
                timeLimit > 0 ? timeLimit : 0, memoryLimit > 0 ? memoryLimit : 0);
            parameters.LanguageHandler = currentLanguageHandler;
            
            return withChecker ? ContestHandler.CheckWithChecker(parameters, checkerFileName) : ContestHandler.Check(parameters);
        }

        private static void DeleteUnnecessaryFiles(string root)
        {
            var directory = new DirectoryInfo(root);
            foreach (var file in directory.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // ignored
                }
            }
        }

        [HttpPost]
        [Route("generate_tests")]
        public IHttpActionResult ExecuteTestGeneration([FromBody]GeneratorInputData input)
        {
            string root = HttpContext.Current.Server.MapPath("~/App_Data");
            var extension = GenerateExtensionFromLanguage(input.Language);

            var generatorFileName = CreateFile(input.Generator, root, "generator", extension);

            var fullTests = new List<int[]>();

            foreach (var currentTestData in input.Tests)
            {
                var currentInputTestFileName = CreateFile(currentTestData.Input, root, "inputtest", extension);
                var currentOutputTestFileName = GenerateFileName(root, "outputtest", extension);

                var errors = GenerateTest(generatorFileName, currentInputTestFileName, currentOutputTestFileName, 
                    input.TimeLimit > 0 ? input.TimeLimit : 0, input.MemoryLimit > 0 ? input.MemoryLimit : 0, input.Language);
                
                if (errors.Length > 0)
                {
                    DeleteUnnecessaryFiles(root);
                    return BadRequest();
                }

                var currentTest = MakeCodesFromByteArray(File.ReadAllBytes(currentOutputTestFileName));
                fullTests.Add(currentTest);
            }

            DeleteUnnecessaryFiles(root);

            return Ok(new OutputData(fullTests.ToArray()));
        }

        private static string GenerateTest(string generatorFileName, string inputTestFileName, string outputTestFileName, 
            int timeLimit, int memoryLimit, Language language)
        {
            LanguageHandler currentLanguageHandler;
            switch (language)
            {
                case Language.CSharp:
                    currentLanguageHandler = new CSharpHandler();
                    break;

                case Language.Cpp:
                    currentLanguageHandler = new CppHandler();
                    break;

                default:
                    currentLanguageHandler = new CSharpHandler();
                    break;
            }

            var parameters = new CheckParametersForGenerator(generatorFileName, inputTestFileName, outputTestFileName,
                timeLimit > 0 ? timeLimit : 0, memoryLimit > 0 ? memoryLimit : 0)
            {
                LanguageHandler = currentLanguageHandler
            };

            return ContestHandler.GenerateTestOutput(parameters);
        }

        public int[] MakeCodesFromByteArray(byte[] array)
        {
            return array.Select(b => int.Parse(b.ToString())).ToArray();
        }
    }
}