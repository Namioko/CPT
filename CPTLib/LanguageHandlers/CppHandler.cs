using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CPTLib.Models.ContestObjects.CheckParameters;

namespace CPTLib.LanguageHandlers
{
    public class CppHandler : LanguageHandler
    {
        private string _command = "";

        public override bool Compile(string path, ref string errors)
        {
            string args = @"/EHsc";
            string extension = Path.GetExtension(path);
            string fileName = Path.GetFileName(path);
            string directory = Path.GetDirectoryName(path);

            if (extension == null || extension != ".cpp")
            {
                errors += "File error: Wrong extension";
                return false;
            }

            if (!File.Exists(path))
            {
                errors += "File error: Wrong file path";
                return false;
            }

            string exe = path.Substring(0, path.Length - extension.Length + 1) + "exe";
            string obj = Path.GetDirectoryName(path) + @"\c++\"
                + fileName.Substring(0, fileName.Length - extension.Length + 1) + "obj";

            if (File.Exists(exe))
            {
                try
                {
                    File.Delete(exe);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Process proc = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = "cmd.exe";
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;

            proc.StartInfo = info;
            proc.Start();

            using (StreamWriter sw = proc.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    sw.WriteLine("cd " + directory);
                    sw.WriteLine(@"cd c++");
                    sw.WriteLine("vcvars32.bat");
                    this._command = "cl.exe " + args + " " + path + " /link /out:" + exe;
                    sw.WriteLine(this._command);
                    if (File.Exists(obj))
                    {
                        sw.WriteLine("del " + obj);
                    }
                }
            }

            var output = proc.StandardOutput.ReadToEnd();

            proc.WaitForExit();

            bool success = File.Exists(exe);
            if (!success)
            {
                errors += this.GetErrorsFromOutput(output, path);
            }

            return success;
        }

        private string GetErrorsFromOutput(string output, string path)
        {
            var currDirectory = Path.GetDirectoryName(path);
            var fileName = Path.GetFileName(path);

            var index = output.IndexOf(this._command);
            var preErrors = output.Substring(index + this._command.Length + 1 + fileName.Length + 1);
            preErrors = preErrors.Replace(path, "");

            preErrors = preErrors.Replace(currDirectory + @"\C++>", "");
            preErrors = (new Regex("\\r\\n")).Replace(preErrors, "", 1);
            return preErrors.Replace("\r\n\r\n", "");
        }

        public override bool Execute(CheckParameters parameters, ref string output, ref string errors, ref double usedTime, ref double usedMemory, bool isChecker)
        {
            var extension = Path.GetExtension(parameters.FileName);
            var executableFilePath = parameters.FileName.Substring(0, parameters.FileName.Length - extension.Length) + ".exe";

            var inputFilePath = isChecker
                ? ((CheckParametersForChecker)parameters).SolutionOutputFileName
                : parameters.InputTestFileName;

            var timeLimit = parameters.TimeLimit;
            if (timeLimit == 0)
            {
                timeLimit = 120; //2 mins
            }

            if (!File.Exists(executableFilePath))
            {
                errors += "If you see this message, please contact administrator (ExErr1)";
                return false;
            }
            if (!File.Exists(inputFilePath))
            {
                errors += "If you see this message, please contact administrator (ExErr2)";
                return false;
            }
            if (isChecker)
            {
                if (!File.Exists(parameters.InputTestFileName))
                {
                    errors += "If you see this message, please contact administrator (ExErr3)";
                    return false;
                }
                if (!File.Exists(parameters.OutputTestFileName))
                {
                    errors += "If you see this message, please contact administrator (ExErr4)";
                    return false;
                }
            }

            Process proc = new Process();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = executableFilePath;
            info.RedirectStandardInput = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.Arguments = inputFilePath + " " + parameters.InputTestFileName + " " + parameters.OutputTestFileName;

            proc.StartInfo = info;
            proc.Start();

            using (StreamWriter sw = proc.StandardInput)
            {
                if (sw.BaseStream.CanWrite)
                {
                    var sr = new StreamReader(inputFilePath);
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        proc.StandardInput.WriteLine(line);
                    }

                    sr.Close();
                }
            }

            var stdout = "";
            var stderr = "";
            double tempUsedTime = 0;
            double tempUsedMemory = 0;
            bool hasStarted = false;
            bool hasExited = false;
            try
            {
                var t1 = new Task(() => WaitForExit(proc, timeLimit, ref stdout, ref stderr, ref hasStarted));

                var t2 =
                    new Task(
                        () =>
                            CheckTimeLimit(proc, timeLimit, parameters.MemoryLimit, ref tempUsedTime, ref tempUsedMemory, ref stderr,
                                ref hasExited, ref hasStarted));
                t2.Start();
                Thread.Sleep(10);
                t1.Start();

                Task.WaitAll(t1, t2);
            }
            catch (Exception ex)
            {
                errors += ex.Message;
            }

            if (timeLimit < tempUsedTime)
            {
                errors += "Time limit (" + timeLimit + "s) exceeded\r\n";
            }

            if (parameters.MemoryLimit != 0 && parameters.MemoryLimit * 1024 < tempUsedMemory)
            {
                errors += "Memory limit (" + parameters.MemoryLimit + "Kb) exceeded\r\n";
            }

            proc.Close();

            output += stdout;
            errors += stderr;

            usedTime = tempUsedTime;
            usedMemory = tempUsedMemory;

            if (File.Exists(executableFilePath))
            {
                try
                {
                    File.Delete(executableFilePath);
                }
                catch (Exception) { }
            }

            return hasExited && errors == "";
        }

        private void WaitForExit(Process proc, int timeLimit, ref string output, ref string errors, ref bool hasStarted)
        {
            hasStarted = true;
            try
            {
                var tempOutput = proc.StandardOutput.ReadToEnd();
                if (tempOutput.Length >= 2)
                {
                    output += tempOutput.Substring(0, tempOutput.Length - 2); //\r\n
                }
                else
                {
                    output += tempOutput;
                }
                errors += proc.StandardError.ReadToEnd();
            }
            catch (Exception e)
            {
                errors += e.Message;
            }
        }

        private void CheckTimeLimit(Process proc, int timeLimit, int memoryLimit, ref double usedTime, ref double usedMemory, ref string stderr, ref bool hasExited, ref bool hasStarted)
        {
            while (!hasStarted) { }

            while (!proc.HasExited)
            {
                usedTime = proc.TotalProcessorTime.TotalMilliseconds / 1000;
                usedMemory = (double)proc.WorkingSet64 / (1024 * 1024);

                if (timeLimit <= proc.TotalProcessorTime.TotalMilliseconds / 1000)
                {
                    stderr += "Time limit exceeded";
                    usedTime = proc.TotalProcessorTime.TotalMilliseconds / 1000;
                    usedMemory = (double)proc.WorkingSet64 / (1024 * 1024);
                    proc.Kill();
                    hasExited = true;
                    return;
                }

                if (memoryLimit != 0 && memoryLimit <= (double)proc.WorkingSet64 / (1024 * 1024))
                {
                    stderr += "Memory limit exceeded";
                    usedTime = proc.TotalProcessorTime.TotalMilliseconds / 1000;
                    usedMemory = (double)proc.WorkingSet64 / (1024 * 1024);
                    proc.Kill();
                    hasExited = true;
                    return;
                }
            }
            hasExited = proc.HasExited;

            if (!proc.HasExited)
            {
                proc.Kill();
                hasExited = true;
            }
        }
    }
}
