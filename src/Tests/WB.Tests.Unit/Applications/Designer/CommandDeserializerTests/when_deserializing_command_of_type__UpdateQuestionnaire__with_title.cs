using System;
using Machine.Specifications;
using Ncqrs.Commanding;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Applications.Designer.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__UpdateQuestionnaire__with_title : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            commandType = "UpdateQuestionnaire";

            title = @"<b width='7'>MA</b><font color='red'>IN</font><img /><script>alert('hello world!')</script><script/>";
            questionnaireId = "11111111-1111-1111-1111-111111111111";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""title"": ""{1}""
            }}", questionnaireId, title);

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            result = deserializer.Deserialize(commandType, command);

        It should_return_NewUpdateGroupCommand = () =>
            result.ShouldBeOfExactType<UpdateQuestionnaireCommand>();

        It should_return_same_title_in_NewUpdateGroupCommand = () =>
            ((UpdateQuestionnaireCommand)result).Title.ShouldEqual(sanitizedTitle);

        It should_return_same_questionnaire_id_in_NewUpdateGroupCommand = () =>
            ((UpdateQuestionnaireCommand)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string title;
        private static string sanitizedTitle = "<b>MA</b><font color=\"red\">IN</font>";
        private static string questionnaireId;
        private static string commandType;
    }
}