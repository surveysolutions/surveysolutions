using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure.Native
{
    [TestOf(typeof(JsonAllTypesSerializer))]
    public class JsonAllTypesSerializerTests
    {
        [Test]
        public void When_serializing_Identity_Should_serialize_it_as_decimal_array_for_backward_compatibility()
        {
            var identity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), 1, 2, 3);
            JsonAllTypesSerializer serializer = new JsonAllTypesSerializer();
            var str = serializer.Serialize(identity);

            Assert.That(str, Does.Not.Contain("Int32"));
            Assert.That(str, Does.Not.Contain("Decimal"));
            Assert.That(str, Does.Contain("WB.Core.SharedKernels.DataCollection.RosterVector, WB.Core.SharedKernels.DataCollection.Portable"));
        }

        [Test]
        public void When_serializing_roster_vector_array_Should_be_able_to_deserialize_it()
        {
            var rosterVector = Create.RosterVector(3, 4);
            RosterVector[] vectors = {Create.RosterVector(1, 2), rosterVector};
            JsonAllTypesSerializer serializer = new JsonAllTypesSerializer();
            
            string serializedValue = serializer.Serialize(vectors);
            RosterVector[] deserializedValue = serializer.Deserialize<RosterVector[]>(serializedValue);

            Assert.That(deserializedValue.Length, Is.EqualTo(2));
            Assert.That(deserializedValue[1], Is.EqualTo(rosterVector));
        }

        [Test]
        public void Should_be_able_to_serialize_KeyValuePair_of_validation_conditions()
        {
            Dictionary<Identity, IList<FailedValidationCondition>> conditions =  new Dictionary<Identity, IList<FailedValidationCondition>>();
            var identity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), 1, 2, 3);
            var failedValidationCondition = Create.Entity.FailedValidationCondition(1);
            conditions[identity] = new List<FailedValidationCondition> {failedValidationCondition};

            JsonAllTypesSerializer serializer = new JsonAllTypesSerializer();

            IList<KeyValuePair<Identity, IList<FailedValidationCondition>>> keyValuePairs = conditions.ToList();

            var serialize = serializer.Serialize(keyValuePairs);

            Assert.That(serialize, Is.Not.Empty);

            List<KeyValuePair<Identity, IList<FailedValidationCondition>>> deserialized =
                serializer.Deserialize<List<KeyValuePair<Identity, IList<FailedValidationCondition>>>>(serialize);

            Assert.That(deserialized.Count, Is.EqualTo(1));
            Assert.That(deserialized[0].Key, Is.EqualTo(identity));
            Assert.That(deserialized[0].Value[0], Is.EqualTo(failedValidationCondition));
        }
    }
}