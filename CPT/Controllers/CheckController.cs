using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Http;
using CPTLib;
using CPTLib.LanguageHandlers;
using CPTLib.Models;
using CPTLib.Models.Enums;

namespace CPT.Controllers
{
    [RoutePrefix("api/check")]
    public class CheckController : ApiController
    {
        [HttpPost]
        [Route("")]
        public IHttpActionResult ExecuteSolution([FromBody]InputData input)
        {
            string root = HttpContext.Current.Server.MapPath("~/App_Data");

            var extension = input.Language == Language.CSharp ? "cs" : input.Language == Language.Cpp ? "cpp" : "cs";
            var solutionFileName = root + @"\" + "solution." + extension;
            var checkerFileName = root + @"\" + "checker." + extension;

            File.WriteAllBytes(solutionFileName, input.Solution);
            File.WriteAllBytes(checkerFileName, input.Checker);
            
            var testResults = new List<TestResult>();

            for (var i = 0; i < input.Tests.Length; i++)
            {
                var currentTestData = input.Tests[i];

                var currentInputTestFileName = root + @"\" + "inputtest." + extension;
                File.WriteAllBytes(currentInputTestFileName, currentTestData.Input);

                var currentTestOutputFileName = root + @"\" + "outputtest."  + extension;
                File.WriteAllBytes(currentTestOutputFileName, currentTestData.Output);

                double usedTime = 0;
                double usedMemory = 0;
                TestResult currentTestResult;
                if (input.Language == Language.CSharp)
                {
                    currentTestResult = CheckHandler.CheckWithChecker(solutionFileName, checkerFileName, currentInputTestFileName,
                        currentTestOutputFileName, input.TimeLimit, input.MemoryLimit, ref usedTime, ref usedMemory, new CSharpHandler());
                }
                else
                {
                    currentTestResult = CheckHandler.CheckWithChecker(solutionFileName, checkerFileName, currentInputTestFileName,
                        currentTestOutputFileName, input.TimeLimit, input.MemoryLimit, ref usedTime, ref usedMemory, new CppHandler());
                }

                currentTestResult.Number = currentTestData.Number;
                testResults.Add(currentTestResult);

                if (currentTestResult.Shortening == ResultMessage.CE)
                {
                    break;
                }
            }

            var directory = new DirectoryInfo(root);
            foreach (var file in directory.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch(Exception) { }
            }
            
            return Ok(testResults.ToArray());
        }
    }
}