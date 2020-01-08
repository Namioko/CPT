using MongoDB.Bson;

namespace CPTLib.Models.ContestObjects
{
    public class Test
    {
        public byte[] Input { get; }

        public byte[] Output { get; }

        public int Number { get; }

        public Test(byte[] input, byte[] output, int number)
        {
            Input = input;
            Output = output;
            Number = number;
        }

        public static explicit operator Test(BsonDocument v)
        {
            var test = new Test
            (
                v.GetValue("input").AsByteArray,
                v.GetValue("output").AsByteArray,
                v.GetValue("number").AsInt32
            );

            return test;
        }
    }
}
