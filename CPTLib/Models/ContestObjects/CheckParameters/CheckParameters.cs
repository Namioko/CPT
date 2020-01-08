using CPTLib.LanguageHandlers;

namespace CPTLib.Models.ContestObjects.CheckParameters
{
    public abstract class CheckParameters
    {
        public string FileName { get; }
        public string InputTestFileName { get; }
        public string OutputTestFileName { get; }
        public int TimeLimit { get; }
        public int MemoryLimit { get; }
        public LanguageHandler LanguageHandler { get; set; }

        public CheckParameters(string fileName, string inputTestFileName, string outputTestFileName, int timeLimit, int memoryLimit)
        {
            FileName = fileName;
            InputTestFileName = inputTestFileName;
            OutputTestFileName = outputTestFileName;
            TimeLimit = timeLimit;
            MemoryLimit = memoryLimit;
        }
    }
}
