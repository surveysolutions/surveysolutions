using NUnit.Framework;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_unknown_type : CommandDeserializerTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_CommandDeserializationException () {
            var type = "Unknown";

            var command = "{}";

            var deserializer = CreateCommandDeserializer();

            Assert.Throws<CommandDeserializationException>(() => deserializer.Deserialize(type, command));
        }
    }
}
