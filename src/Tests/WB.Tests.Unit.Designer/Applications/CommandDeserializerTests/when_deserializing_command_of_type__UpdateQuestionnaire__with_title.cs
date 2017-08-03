using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__UpdateQuestionnaire__with_title : CommandDeserializerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            commandType = "UpdateQuestionnaire";

            title = @"<b width='7'>MA</b><font color='red'>IN</font><img /><script>alert('hello world!')</script><script/> привет! :)";
            
            questionnaireId = "11111111-1111-1111-1111-111111111111";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""title"": ""{1}""
            }}", questionnaireId, title);

            deserializer = CreateCommandDeserializer();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = deserializer.Deserialize(commandType, command);

        [NUnit.Framework.Test] public void should_return_NewUpdateGroupCommand () =>
            result.ShouldBeOfExactType<UpdateQuestionnaire>();

        [NUnit.Framework.Test] public void should_return_same_title_in_NewUpdateGroupCommand () =>
            ((UpdateQuestionnaire)result).Title.ShouldEqual(sanitizedTitle);

        [NUnit.Framework.Test] public void should_return_same_questionnaire_id_in_NewUpdateGroupCommand () =>
            ((UpdateQuestionnaire)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string command;
        private static string title;
        private static string sanitizedTitle = "<b>MA</b><font color=\"red\">IN</font>alert('hello world!') привет! :)";
        private static string questionnaireId;
        private static string commandType;
    }
}