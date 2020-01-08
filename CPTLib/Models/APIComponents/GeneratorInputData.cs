using System;
using CPTLib.Models.ContestObjects;
using CPTLib.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CPTLib.Models.APIComponents
{
    public class GeneratorInputData
    {
        public byte[] Generator { get; }

        [BsonRepresentation(BsonType.String)]
        public Language Language { get; }

        public int TimeLimit { get; }

        public int MemoryLimit { get; }

        public TestWithoutOutput[] Tests { get; }

        public GeneratorInputData(byte[] generator, Language language, int timeLimit, int memoryLimit, TestWithoutOutput[] tests)
        {
            Generator = generator;
            Language = language;
            TimeLimit = timeLimit;
            MemoryLimit = memoryLimit;
            Tests = tests;
        }

        public static explicit operator GeneratorInputData(BsonDocument v)
        {
            Language language;
            Enum.TryParse(v.GetValue("language").AsString, true, out language);

            var problem = new GeneratorInputData
            (
                v.GetValue("generator").AsByteArray,
                language,
                v.GetValue("timeLimit").AsInt32,
                v.GetValue("memoryLimit").AsInt32,
                AggregateTestData(v.GetValue("tests").AsBsonArray)
            );

            return problem;
        }

        private static TestWithoutOutput[] AggregateTestData(BsonArray array)
        {
            var data = new TestWithoutOutput[array.Count];
            for (var i = 0; i < array.Count; i++)
            {
                data[i] = (TestWithoutOutput)array[i].AsBsonDocument;
            }
            return data;
        }
    }
}