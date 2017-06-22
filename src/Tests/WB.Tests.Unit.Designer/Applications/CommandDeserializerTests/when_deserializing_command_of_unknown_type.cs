using System;
using Machine.Specifications;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_unknown_type : CommandDeserializerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            type = "Unknown";

            command = "{}";

            deserializer = CreateCommandDeserializer();
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                deserializer.Deserialize(type, command));

        [NUnit.Framework.Test] public void should_throw_CommandDeserializationException () =>
            exception.ShouldBeOfExactType<CommandDeserializationException>();

        private static Exception exception;
        private static CommandDeserializer deserializer;
        private static string type;
        private static string command;
    }
}