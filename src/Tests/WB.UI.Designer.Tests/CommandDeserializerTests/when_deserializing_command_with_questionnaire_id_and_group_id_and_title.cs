namespace WB.UI.Designer.Tests.CommandDeserializerTests
{
    using System;

    using Machine.Specifications;

    using Main.Core.Commands.Questionnaire.Group;

    using Ncqrs.Commanding;

    using WB.UI.Designer.Code.Helpers;

    internal class when_deserializing_command_with_questionnaire_id_and_group_id_and_title : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            title = "MAIN";
            questionnaireId = "11111111-1111-1111-1111-111111111111";
            groupId = "22222222-2222-2222-2222-222222222222";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""groupId"": ""{1}"",
                ""title"": ""{2}""
            }}", questionnaireId, groupId, title);

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            result = deserializer.Deserialize(command);

        It should_return_NewUpdateGroupCommand = () =>
            result.ShouldBeOfType<NewUpdateGroupCommand>();

        It should_return_same_title_in_NewUpdateGroupCommand = () =>
            ((NewUpdateGroupCommand)result).Title.ShouldEqual(title);

        It should_return_same_questionnaire_id_in_NewUpdateGroupCommand = () =>
            ((NewUpdateGroupCommand)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        It should_return_same_group_id_in_NewUpdateGroupCommand = () =>
            ((NewUpdateGroupCommand)result).GroupId.ShouldEqual(Guid.Parse(groupId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string title;
        private static string questionnaireId;
        private static string groupId;
    }
}