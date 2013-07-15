using WB.UI.Shared.Web.CommandDeserialization;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

namespace WB.UI.Designer.Tests.CommandDeserializerTests
{
    using System;

    using Machine.Specifications;
    using Ncqrs.Commanding;

    using WB.UI.Designer.Code.Helpers;

    internal class when_deserializing_command_of_type__DeleteQuestion__with_questionnaire_id_and_question_id : CommandDeserializerTestsContext
    {
        Establish context = () =>
        {
            type = "DeleteQuestion";

            questionnaireId = "11111111-1111-1111-1111-111111111111";
            questionId = "22222222-2222-2222-2222-222222222222";

            command = string.Format(@"{{
                ""questionnaireId"": ""{0}"",
                ""questionId"": ""{1}""
            }}", questionnaireId, questionId);

            deserializer = CreateCommandDeserializer();
        };

        Because of = () =>
            result = deserializer.Deserialize(type, command);

        It should_return_NewDeleteQuestionCommand = () =>
            result.ShouldBeOfType<DeleteQuestionCommand>();

        It should_return_same_questionnaire_id_in_NewDeleteQuestionCommand = () =>
            ((DeleteQuestionCommand)result).QuestionnaireId.ShouldEqual(Guid.Parse(questionnaireId));

        It should_return_same_question_id_in_NewDeleteQuestionCommand = () =>
            ((DeleteQuestionCommand)result).QuestionId.ShouldEqual(Guid.Parse(questionId));

        private static ICommand result;
        private static CommandDeserializer deserializer;
        private static string questionnaireId;
        private static string questionId;
        private static string type;
        private static string command;
    }
}