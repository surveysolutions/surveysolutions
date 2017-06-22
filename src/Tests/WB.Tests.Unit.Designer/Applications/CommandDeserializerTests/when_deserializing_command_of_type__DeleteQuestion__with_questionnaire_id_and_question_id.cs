using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class when_deserializing_command_of_type__DeleteQuestion__with_questionnaire_id_and_question_id : CommandDeserializerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            type = "DeleteQuestion";

            questionnaireId = "11111111-1111-1111-1111-111111111111";
            questionId = "22222222-2222-2222-2222-222222222222";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""questionId"": ""{1}""
            }}", questionnaireId, questionId);

            deserializer = CreateCommandDeserializer();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = deserializer.Deserialize(type, command);

        [NUnit.Framework.Test] public void should_return_NewDeleteQuestionCommand () =>
            result.ShouldBeOfExactType<DeleteQuestion>();

        [NUnit.Framework.Test] public void should_return_same_questionnaire_id_in_NewDeleteQuestionCommand () =>
            ((DeleteQuestion)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        [NUnit.Framework.Test] public void should_return_same_question_id_in_NewDeleteQuestionCommand () =>
            ((DeleteQuestion)result).QuestionId.ShouldEqual(Guid.Parse(questionId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string questionnaireId;
        private static string questionId;
        private static string type;
        private static string command;
    }
}