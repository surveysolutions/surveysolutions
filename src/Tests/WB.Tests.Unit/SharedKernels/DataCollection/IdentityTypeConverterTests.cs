using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestFixture]
    public class IdentityTypeConverterTests
    {
        [Test]
        public void Should_support_conversion_of_identity_type()
        {
            IdentityTypeConverter converter = new IdentityTypeConverter();

            bool canConvert = converter.CanConvert(typeof(Identity));

            Assert.That(canConvert, Is.True);
        }

        [Test]
        public void Should_serialize()
        {
            IdentityTypeConverter converter = new IdentityTypeConverter();
            var guid = Guid.Parse("668caf6e-4893-4c3f-a718-39dacec59393");
            var identity = Create.Identity(guid, Create.RosterVector(1, 2, 3));

            var stringBuilder = new StringBuilder();
            StringWriter textWriter = new StringWriter(stringBuilder);
            JsonWriter writer = new JsonTextWriter(textWriter);

            converter.WriteJson(writer, identity, new JsonSerializer {Formatting = Formatting.None});
            var jsonString = stringBuilder.ToString();


            Assert.That(jsonString, Is.EqualTo(@"{""id"":""668caf6e48934c3fa71839dacec59393"",""rosterVector"":[""1"",""2"",""3""]}"));
        }

        [Test]
        public void Should_deserialize()
        {
            IdentityTypeConverter converter = new IdentityTypeConverter();

            StringReader textReader = new StringReader(@"{""id"":""668caf6e48934c3fa71839dacec59393"",""rosterVector"":[""1"",""2"",""3""]}");
            JsonReader reader = new JsonTextReader(textReader);
            Identity deserializedIdentity = (Identity) converter.ReadJson(reader, typeof(Identity), null, new JsonSerializer());

            Assert.That(deserializedIdentity.Id, Is.EqualTo(Guid.Parse("668caf6e48934c3fa71839dacec59393")));
            Assert.That(deserializedIdentity.RosterVector, Is.EqualTo(Create.RosterVector(1, 2, 3)));
        }
    }
}