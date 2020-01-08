using System;

namespace CPTLib.Models.APIComponents
{
    public class OutputData
    {
        public Array Results { get; }

        public OutputData(Array results)
        {
            Results = results;
        }
    }
}