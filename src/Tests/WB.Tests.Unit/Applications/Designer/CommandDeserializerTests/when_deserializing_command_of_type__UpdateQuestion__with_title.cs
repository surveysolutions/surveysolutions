using System;
using Machine.Specifications;
using Ncqrs.Commanding;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Text;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Applications.Designer.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__UpdateQuestion__with_title : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            commandType = "UpdateTextQuestion";

            title = @"<b width='7'>MA</b><font color='red'>IN</font><img /><script>alert('hello world!')</script><script/>";
            questionnaireId = "11111111-1111-1111-1111-111111111111";
            questionId = "22222222-2222-2222-2222-222222222222";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""questionId"": ""{1}"",
                ""title"": ""{2}""
            }}", questionnaireId, questionId, title);

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            result = deserializer.Deserialize(commandType, command);

        It should_return_NewUpdateGroupCommand = () =>
            result.ShouldBeOfExactType<UpdateTextQuestionCommand>();

        It should_return_same_title_in_NewUpdateGroupCommand = () =>
            ((UpdateTextQuestionCommand)result).Title.ShouldEqual(sanitizedTitle);

        It should_return_same_questionnaire_id_in_NewUpdateGroupCommand = () =>
            ((UpdateTextQuestionCommand)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        It should_return_same_group_id_in_NewUpdateGroupCommand = () =>
            ((UpdateTextQuestionCommand)result).QuestionId.ShouldEqual(Guid.Parse(questionId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string title;
        private static string sanitizedTitle = "<b>MA</b><font color=\"red\">IN</font>";
        private static string questionnaireId;
        private static string questionId;
        private static string commandType;
    }
}