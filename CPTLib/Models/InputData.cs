using System;
using CPTLib.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CPTLib.Models
{
    public class InputData
    {
        public byte[] Solution { get; set; }

        public byte[] Checker { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Language Language { get; set; }

        public int TimeLimit { get; set; }

        public int MemoryLimit { get; set; }

        public Test[] Tests { get; set; }

        public static explicit operator InputData(BsonDocument v)
        {
            Language language;
            Enum.TryParse(v.GetValue("language").AsString, true, out language);

            var problem = new InputData
            {
                Solution = v.GetValue("solution").AsByteArray,
                Checker = v.GetValue("checker").AsByteArray,
                Language = language,
                TimeLimit = v.GetValue("timeLimit").AsInt32,
                MemoryLimit = v.GetValue("memoryLimit").AsInt32,
                Tests = AggregateTestData(v.GetValue("tests").AsBsonArray)
            };

            return problem;
        }

        private static Test[] AggregateTestData(BsonArray array)
        {
            var data = new Test[array.Count];
            for (var i = 0; i < array.Count; i++)
            {
                data[i] = (Test)array[i].AsBsonDocument;
            }
            return data;
        }
    }
}
