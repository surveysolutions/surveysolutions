using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_single_option_question : QuestionDataParserTestContext
    {
        Establish context = () => { questionDataParser = CreateQuestionDataParser(); };

        private Because of =
            () =>
                result =
                    questionDataParser.Parse(answer, questionVarName,
                        CreateQuestionnaireDocumentWithOneChapter(new SingleQuestion()
                        {
                            PublicKey = questionId,
                            QuestionType = QuestionType.SingleOption,
                            Answers = new List<Answer>() { new Answer() { AnswerValue = "1", AnswerText = "1" }, new Answer() { AnswerValue = "2", AnswerText = "2" } },
                            StataExportCaption = questionVarName
                        }));

        It should_result_be_equal_to_2 = () =>
            result.Value.Value.ShouldEqual((decimal)2);

        It should_result_key_be_equal_to_questionId = () =>
            result.Value.Key.ShouldEqual(questionId);

        private static QuestionDataParser questionDataParser;
        private static KeyValuePair<Guid, object>? result;
        private static Guid questionId = Guid.NewGuid();
        private static string questionVarName = "var";
        private static string answer = "2";
    }
}
