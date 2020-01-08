namespace CPTLib.Models.ContestObjects.CheckParameters
{
    public class CheckParametersForSolution: CheckParameters
    {
        public CheckParametersForSolution(string fileName, string inputTestFileName, string outputTestFileName, int timeLimit,
            int memoryLimit) : base(fileName, inputTestFileName, outputTestFileName, timeLimit, memoryLimit) { }
    }
}
