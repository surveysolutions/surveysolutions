using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.Tests.CommandDeserializerTests
{
    using System;

    using Machine.Specifications;

    using Main.Core.Commands.Questionnaire.Group;

    using Ncqrs.Commanding;

    using WB.UI.Designer.Code.Helpers;

    internal class when_deserializing_command_of_type__DeleteGroup__with_questionnaire_id_and_group_id : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            type = "DeleteGroup";

            questionnaireId = "11111111-1111-1111-1111-111111111111";
            groupId = "22222222-2222-2222-2222-222222222222";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""groupId"": ""{1}""
            }}", questionnaireId, groupId);

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            result = deserializer.Deserialize(type, command);

        It should_return_NewDeleteGroupCommand = () =>
            result.ShouldBeOfType<DeleteGroupCommand>();

        It should_return_same_questionnaire_id_in_NewDeleteGroupCommand = () =>
            ((DeleteGroupCommand)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        It should_return_same_group_id_in_NewDeleteGroupCommand = () =>
            ((DeleteGroupCommand)result).GroupId.ShouldEqual(Guid.Parse(groupId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string questionnaireId;
        private static string groupId;
        private static string type;
        private static string command;
    }
}