namespace CPTLib.Models.ContestObjects.CheckParameters
{
    public class CheckParametersForGenerator: CheckParameters
    {
        public CheckParametersForGenerator(string fileName, string inputTestFileName, string outputTestFileName, int timeLimit,
            int memoryLimit) : base(fileName, inputTestFileName, outputTestFileName, timeLimit, memoryLimit) { }
    }
}
