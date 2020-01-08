using System;
using CPTLib.Models.ContestObjects;
using CPTLib.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CPTLib.Models.APIComponents
{
    public class InputData
    {
        public byte[] Solution { get; }

        public byte[] Checker { get; }

        [BsonRepresentation(BsonType.String)]
        public Language Language { get; }

        public int TimeLimit { get; }

        public int MemoryLimit { get; }

        public Test[] Tests { get; }

        public InputData(byte[] solution, byte[] checker, Language language, int timeLimit, int memoryLimit, Test[] tests)
        {
            Solution = solution;
            Checker = checker;
            Language = language;
            TimeLimit = timeLimit;
            MemoryLimit = memoryLimit;
            Tests = tests;
        }

        public static explicit operator InputData(BsonDocument v)
        {
            Language language;
            Enum.TryParse(v.GetValue("language").AsString, true, out language);

            var problem = new InputData
            (
                v.GetValue("solution").AsByteArray,
                v.GetValue("checker").AsByteArray,
                language,
                v.GetValue("timeLimit").AsInt32,
                v.GetValue("memoryLimit").AsInt32,
                AggregateTestData(v.GetValue("tests").AsBsonArray)
            );

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