namespace CPTLib.LanguageHandlers
{
    public abstract class LanguageHandler
    {
        public abstract bool Compile(string path, ref string errors);

        public abstract bool Execute(string executableFilePath, string inputFilePath, ref string output, ref string errors,
            int timeLimit, int memoryLimit, ref double usedTime, ref double usedMemory, bool isChecker, string inputTestFilePath = "",
            string outputTestFilePath = "");
    }
}
