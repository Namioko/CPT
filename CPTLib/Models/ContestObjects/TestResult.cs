using CPTLib.Models.Enums;

namespace CPTLib.Models.ContestObjects
{
    public class TestResult
    {
        public string Message { get; }
        public ResultMessage Shortening { get; }
        public int Number { get; set; }

        public double Time { get; }
        public double Memory { get; }

        public TestResult(string message, ResultMessage shortening, double usedTime, double usedMemory)
        {
            Message = message;
            Shortening = shortening;
            Time = usedTime;
            Memory = usedMemory;
        }
    }
}
