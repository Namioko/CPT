using CPTLib.Models.ContestObjects.CheckParameters;

namespace CPTLib.LanguageHandlers
{
    public abstract class LanguageHandler
    {
        public abstract bool Compile(string path, ref string errors);

        public abstract bool Execute(CheckParameters parameters, ref string output, ref string errors, ref double usedTime, 
            ref double usedMemory, bool isChecker);
    }
}
