using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_single_option_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "2";
            questionDataParser = CreateQuestionDataParser();
            question = new SingleQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.SingleOption,
                Answers =
                    new List<Answer>()
                    {
                        new Answer() { AnswerValue = "1", AnswerText = "1" },
                        new Answer() { AnswerValue = "2", AnswerText = "2" }
                    },
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName, question, CreateQuestionnaireDocumentWithOneChapter(question), out parcedValue);

        private It should_result_be_equal_to_2 = () =>
            parcedValue.Value.ShouldEqual((decimal) 2);

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}