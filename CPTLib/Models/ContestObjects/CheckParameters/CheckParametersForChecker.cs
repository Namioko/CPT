namespace CPTLib.Models.ContestObjects.CheckParameters
{
    public class CheckParametersForChecker: CheckParameters
    {
        public string SolutionOutputFileName { get; }

        public CheckParametersForChecker(string fileName, string solutionOutputFileName, string inputTestFileName, 
            string outputTestFileName, int timeLimit, int memoryLimit) : 
            base(fileName, inputTestFileName, outputTestFileName, timeLimit, memoryLimit)
        {
            SolutionOutputFileName = solutionOutputFileName;
        }

        public CheckParametersForChecker(CheckParameters parameters, string checkerFileName, string solutionOutputFileName) :
            base(checkerFileName, parameters.InputTestFileName, parameters.OutputTestFileName, parameters.TimeLimit, 
                parameters.MemoryLimit)
        {
            SolutionOutputFileName = solutionOutputFileName;
            LanguageHandler = parameters.LanguageHandler;
        }
    }
}
