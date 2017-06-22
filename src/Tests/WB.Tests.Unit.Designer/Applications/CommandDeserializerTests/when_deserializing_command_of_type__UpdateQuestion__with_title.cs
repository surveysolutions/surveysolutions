using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__UpdateQuestion__with_title : CommandDeserializerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            commandType = "UpdateTextQuestion";

            title = @"<b width='7'>MA</b><font color='red'>IN</font><img /><script>alert('hello world!')</script><script/>";
            questionnaireId = "11111111-1111-1111-1111-111111111111";
            questionId = "22222222-2222-2222-2222-222222222222";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""questionId"": ""{1}"",
                ""commonQuestionParameters"": {{
                    ""title"": ""{2}""
                }}
            }}", questionnaireId, questionId, title);

            deserializer = CreateCommandDeserializer();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = deserializer.Deserialize(commandType, command);

        [NUnit.Framework.Test] public void should_return_NewUpdateGroupCommand () =>
            result.ShouldBeOfExactType<UpdateTextQuestion>();

        [NUnit.Framework.Test] public void should_return_same_title_in_NewUpdateGroupCommand () =>
            ((UpdateTextQuestion)result).Title.ShouldEqual(sanitizedTitle);

        [NUnit.Framework.Test] public void should_return_same_questionnaire_id_in_NewUpdateGroupCommand () =>
            ((UpdateTextQuestion)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        [NUnit.Framework.Test] public void should_return_same_group_id_in_NewUpdateGroupCommand () =>
            ((UpdateTextQuestion)result).QuestionId.ShouldEqual(Guid.Parse(questionId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string title;
        private static string sanitizedTitle = "<b>MA</b><font color=\"red\">IN</font>alert('hello world!')";
        private static string questionnaireId;
        private static string questionId;
        private static string commandType;
    }
}