using System;
using Machine.Specifications;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Applications.Designer.CommandDeserializerTests
{
    internal class when_deserializing_command_of_unknown_type : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            type = "Unknown";

            command = "{}";

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                deserializer.Deserialize(type, command));

        It should_throw_CommandDeserializationException = () =>
            exception.ShouldBeOfExactType<CommandDeserializationException>();

        private static Exception exception;
        private static CommandDeserializer deserializer;
        private static string type;
        private static string command;
    }
}