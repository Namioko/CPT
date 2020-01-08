using MongoDB.Bson;

namespace CPTLib.Models.ContestObjects
{
    public class TestWithoutOutput
    {
        public byte[] Input { get; }

        public int Number { get; }

        public TestWithoutOutput(byte[] input, int number)
        {
            Input = input;
            Number = number;
        }

        public static explicit operator TestWithoutOutput(BsonDocument v)
        {
            var test = new TestWithoutOutput
            (
                v.GetValue("input").AsByteArray,
                v.GetValue("number").AsInt32
            );

            return test;
        }
    }
}
