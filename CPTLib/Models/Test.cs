using MongoDB.Bson;

namespace CPTLib.Models
{
    public class Test
    {
        public byte[] Input { get; set; }

        public byte[] Output { get; set; }

        public int Number { get; set; }

        public static explicit operator Test(BsonDocument v)
        {
            var test = new Test
            {
                Input = v.GetValue("input").AsByteArray,
                Output = v.GetValue("output").AsByteArray,
                Number = v.GetValue("number").AsInt32
            };

            return test;
        }
    }
}
