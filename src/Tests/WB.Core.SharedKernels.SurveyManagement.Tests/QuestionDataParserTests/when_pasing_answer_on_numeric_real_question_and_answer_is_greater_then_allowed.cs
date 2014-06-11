using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_numeric_real_question_and_answer_is_greater_then_allowed : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "5.6";
            questionDataParser = CreateQuestionDataParser();
        };

        Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName,
                        CreateQuestionnaireDocumentWithOneChapter(new NumericQuestion()
                        {
                            PublicKey = questionId,
                            QuestionType = QuestionType.Numeric,
                            IsInteger = false,
                            MaxValue = 3,
                            StataExportCaption = questionVarName
                        }), out parcedValue);

        It should_result_be_equal_to_5_6 = () =>
            parcedValue.Value.ShouldEqual((decimal)5.6);

        It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);

        It should_parsing_result_be_equal_to_AnswerIsIncorrectBecauseIsGreaterThanMaxValue = () =>
          parsingResult.ShouldEqual(ValueParsingResult.AnswerIsIncorrectBecauseIsGreaterThanMaxValue);
    }
}
