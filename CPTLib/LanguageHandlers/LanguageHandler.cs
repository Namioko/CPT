using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;

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
