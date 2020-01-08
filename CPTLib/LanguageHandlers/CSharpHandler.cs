using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CPTLib.LanguageHandlers
{
    public class CSharpHandler : LanguageHandler
    {
        public override bool Compile(string path, ref string errors)
        {
            string args = "/t:exe";
            string extension = Path.GetExtension(path);

            if (extension == null || extension != ".cs")
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
                    sw.WriteLine("cd " + Path.GetDirectoryName(path));
                    sw.WriteLine(@"cd C#");
                    sw.WriteLine("csc.exe " + args + " -out:" + exe + " " + path);
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
            var newPath = path[0].ToString().ToLower() + path.Substring(1);
            var pathRegex = new Regex(@"\(\d+,\d+\): ");

            var match = Regex.Match(output, Regex.Replace(newPath, @"\\", @"\\") + pathRegex);
            var preErrors = output.Substring(match.Index);
            preErrors = preErrors.Replace(newPath, fileName);

            preErrors = preErrors.Replace(currDirectory + @"\C#>", "");
            return preErrors.Replace("\r\n\r\n", "");
        }

        public override bool Execute(string executableFilePath, string inputFilePath, ref string output, ref string errors, int timeLimit,
            int memoryLimit, ref double usedTime, ref double usedMemory, bool isChecker, string inputTestFilePath = "", string outputTestFilePath = "")
        {
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

            bool hasExited = false;
            bool hasLimitExceptions = false;
            int exitCode = 0;
            string stdout = "", stderr = "";
            string limitStderr = "";
            double tempUsedTime = 0;
            double tempUsedMemory = 0;
            var assemblyFileName = Path.GetDirectoryName(inputFilePath).Replace("App_Data", @"bin\CPTLib.dll");

            try
            {
                PermissionSet ps = new PermissionSet(PermissionState.None);
                ps.AddPermission(new UIPermission(PermissionState.Unrestricted));
                ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
                ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
                ps.AddPermission(new FileIOPermission(
                    FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, assemblyFileName));
                ps.AddPermission(new FileIOPermission(FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read, executableFilePath));
                ps.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, inputFilePath));
                if (isChecker)
                {
                    ps.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, inputTestFilePath));
                    ps.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, outputTestFilePath));
                }
                AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
                setup.ShadowCopyFiles = "true";
                var sandbox = AppDomain.CreateDomain("Sandbox", null, setup, ps);
                AppDomain.MonitoringIsEnabled = true;

                TextReader inputFileReader = new StreamReader(inputFilePath);
                string[] arguments = isChecker
                    ? new string[] { inputFilePath, inputTestFilePath, outputTestFilePath }
                    : new string[] { inputFilePath };
                
                try
                {
                    var hookName = typeof (SandboxHook).FullName;
                    ObjectHandle obj = sandbox.CreateInstanceFrom(assemblyFileName, hookName);
                    if (obj == null) throw new ApplicationException("Unable to hook child application domain.");
                    using (SandboxHook hook = (SandboxHook) obj.Unwrap())
                    {
                        hook.Capture(inputFileReader == null ? String.Empty : inputFileReader.ReadToEnd());
                        inputFileReader.Close();
                        
                        var t =
                            new Task(
                                () =>
                                    WaitForExit(sandbox, hook, executableFilePath, arguments, ref stdout, ref stderr,
                                        ref exitCode, ref hasExited));
                        t.Start();
                        var t2 =
                            new Task(
                                () =>
                                    CheckTimeLimit(sandbox, timeLimit, memoryLimit, ref tempUsedTime,
                                        ref tempUsedMemory,
                                        ref limitStderr, ref hasExited, ref hasLimitExceptions));
                        t2.Start();

                        Task.WaitAll(t, t2);
                    }
                }
                finally
                {
                    if (!hasLimitExceptions)
                    {
                        AppDomain.Unload(sandbox);
                    }
                }
            }
            catch (Exception ex)
            {
                errors += hasLimitExceptions ? "" : ex.Message + "\r\n";
            }

            output += stdout;
            errors += hasLimitExceptions ? limitStderr : stderr;

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

            return hasExited && errors == "" && exitCode == 0;
        }

        private void WaitForExit(AppDomain sandbox, SandboxHook hook, string executableFilePath, string[] arguments,
            ref string stdout, ref string stderr, ref int exitCode, ref bool hasExited)
        {
            try
            {
                exitCode = sandbox.ExecuteAssembly(executableFilePath, arguments);
            }
            catch (Exception ex)
            {
                stderr += ex.Message;
            }
            finally
            {
                var tempStderr = "";
                hook.GetOutput(out stdout, out tempStderr);
                stderr += tempStderr;
                hasExited = true;
            }
        }

        private void CheckTimeLimit(AppDomain sandbox, int timeLimit, int memoryLimit, ref double usedTime, ref double usedMemory, ref string stderr, ref bool hasExited, ref bool hasLimitExceptions)
        {
            while (!hasExited)
            {
                if (timeLimit <= sandbox.MonitoringTotalProcessorTime.TotalMilliseconds / 60)
                {
                    ThrowLimitException(sandbox, ref usedTime, ref usedMemory, ref stderr, ref hasLimitExceptions, "Time limit exceeded");
                }

                if (memoryLimit != 0 && memoryLimit <= (double)AppDomain.MonitoringSurvivedProcessMemorySize / (1024 * 1024))
                {
                    ThrowLimitException(sandbox, ref usedTime, ref usedMemory, ref stderr, ref hasLimitExceptions, "Memory limit exceeded");
                }
            }
            
            usedTime = sandbox.MonitoringTotalProcessorTime.TotalMilliseconds / 60;
            usedMemory = (double)AppDomain.MonitoringSurvivedProcessMemorySize / (1024 * 1024);
        }

        private void ThrowLimitException(AppDomain sandbox, ref double usedTime, ref double usedMemory, ref string stderr, ref bool hasLimitExceptions, string errorMessgage)
        {
            usedTime = sandbox.MonitoringTotalProcessorTime.TotalMilliseconds / 60;
            usedMemory = (double)AppDomain.MonitoringSurvivedProcessMemorySize / (1024 * 1024);
            stderr += errorMessgage;
            hasLimitExceptions = true;
            AppDomain.Unload(sandbox);
            GC.Collect();
            return;
        }
    }
}
