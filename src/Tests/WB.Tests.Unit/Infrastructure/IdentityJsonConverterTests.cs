using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Infrastructure
{
    [TestFixture]
    public class IdentityJsonConverterTests
    {
        private string Wrap(string value) => $"\"{value}\"";

        [Test]
        public void ShouldBeAbleToReadJson()
        {
            var id = this.Wrap("id");
            var rosterVector = this.Wrap("rosterVector");
            var json = $@"[
                    {{ 
                       {id}: {this.Wrap("08A3D521-DBF5-4A6A-8D93-6E2A1F9C6EE8")},
                       {rosterVector}: [0.0, 2.0]
                    }},
                    {{ 
                       {id}: {this.Wrap("9D42A4C3-D92A-477C-9688-F5687A598B4B")},
                       {rosterVector}: [0.0]
                    }},
                    {{ 
                       {id}: {this.Wrap("048CA334-7FE9-4A03-9C6A-74BFB2AB7246")},
                       {rosterVector}: []
                    }},
                    {{ 
                       {id}: {this.Wrap("4A56F36B-A234-4DC9-9E54-9876EEF053B7")},
                       {rosterVector}: [23, 12, 12]
                    }}
            ]";

            var result = JsonConvert.DeserializeObject<Identity[]>(json, JsonSettings);

            Assert.That(result[0].Id, Is.EqualTo(Guid.Parse("08A3D521-DBF5-4A6A-8D93-6E2A1F9C6EE8")));
            Assert.That(result[1].Id, Is.EqualTo(Guid.Parse("9D42A4C3-D92A-477C-9688-F5687A598B4B")));
            Assert.That(result[2].Id, Is.EqualTo(Guid.Parse("048CA334-7FE9-4A03-9C6A-74BFB2AB7246")));
            Assert.That(result[3].Id, Is.EqualTo(Guid.Parse("4A56F36B-A234-4DC9-9E54-9876EEF053B7")));

            Assert.That(result[0].RosterVector, Is.EquivalentTo(new[] { 0, 2 }));
            Assert.That(result[1].RosterVector, Is.EquivalentTo(new[] { 0 }));
            Assert.That(result[2].RosterVector, Is.EquivalentTo(new int[0]));
            Assert.That(result[3].RosterVector, Is.EquivalentTo(new[] { 23, 12, 12 }));
        }

        [Test]
        public void ShouldBeAbleToSerializeIdentity()
        {
            var identity = new Identity(Guid.Parse("b4eb6b8b-aeed-488d-9521-ddcdf77c626e"), new RosterVector(new List<int> { 1, 2, 3 }));

            var json = JsonConvert.SerializeObject(identity, JsonSettings);
            var deserialized = JsonConvert.DeserializeObject<Identity>(json, JsonSettings);

            Assert.That(deserialized, Is.EqualTo(identity));
        }

        [Test]
        public void ShouldBeAbleToSerializeObjectsContainingIdentity()
        {
            var answer = new TestAnswer
            {
                Id = Guid.NewGuid(),
                Questions = new Identity[]
                {
                    new Identity(Guid.NewGuid(), new[] {1, 2, 3}),
                    new Identity(Guid.NewGuid(), new[] {1}),
                    new Identity(Guid.NewGuid(), new[] {0}),
                    new Identity(Guid.NewGuid(), new[] {10.0m, 2.0m, 3.0m})
                },
                Option = new int[] { 1, 2, 3 },
                Options = new RosterVector[]
                {
                    new int[] { 1, 2, 3 },
                    new int[] { },
                    new int[] { 1, 2, 3 } }
            };

            var json = JsonConvert.SerializeObject(answer, JsonSettings);
            json = json.Replace("[1,2,3]", "[1.0,2.0,3.0]");
            var answerFromJson = JsonConvert.DeserializeObject<TestAnswer>(json, JsonSettings);
            Assert.That(answer.Id, Is.EqualTo(answerFromJson.Id));
            Assert.That(answer.Questions[2].RosterVector, Is.EqualTo(answerFromJson.Questions[2].RosterVector));
        }

        [Test]
        public void should_not_write_empty_roster_vector()
        {
            var json = JsonConvert.SerializeObject(Create.Identity(Id.g1, RosterVector.Empty), JsonSettings);

            Assert.That(json, Is.EqualTo(@"{""id"":""11111111-1111-1111-1111-111111111111""}"));
        }

        [Test]
        public void when_deserializing_should_use_empty_roster_vector_as_default()
        {
            string json = @"{""id"":""11111111-1111-1111-1111-111111111111""}";
            var id = JsonConvert.DeserializeObject<Identity>(json, JsonSettings);

            Assert.That(id.RosterVector, Is.SameAs(RosterVector.Empty));
        }

        public class TestAnswer
        {
            public Guid Id { get; set; }
            public Identity[] Questions { get; set; }
            public RosterVector[] Options { get; set; }
            public RosterVector Option { get; set; }
        }

        private static JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter(), new IdentityJsonConverter(), new RosterVectorConverter() },
            SerializationBinder = new OldToNewAssemblyRedirectSerializationBinder()
        };
    }
}
