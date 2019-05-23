using System;
using System.IO;

namespace CPTLib.LanguageHandlers
{
    [Serializable]
    class SandboxHook: MarshalByRefObject, IDisposable
    {
        TextWriter _out, _err;
        TextReader _in;

        StringWriter _captureout, _captureerr;

        public SandboxHook() { }

        public void Capture(string stdInput)
        {
            _out = Console.Out;
            _err = Console.Error;
            _in = Console.In;

            Console.SetOut(_captureout = new StringWriter());
            Console.SetError(_captureerr = new StringWriter());
            Console.SetIn(new StringReader(stdInput ?? String.Empty));
        }

        public void GetOutput(out string stdout, out string stderr)
        {
            Console.SetIn(_in);
            _in = null;
            Console.SetOut(_out);
            Console.SetError(_err);
            _out = _err = null;
            stdout = _captureout.ToString();
            _captureout.Dispose();
            _captureout = null;
            stderr = _captureerr.ToString();
            _captureerr.Dispose();
            _captureerr = null;
        }

        public void Dispose() { }
    }
}
