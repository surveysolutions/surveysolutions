using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_multy_option_question_and_answer_cant_be_mapped_on_any_option : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
            question = new MultyOptionsQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.MultyOption,
                StataExportCaption = questionVarName,
                Answers = new List<Answer>
                {
                    Create.Entity.Answer("foo", 3m)
                }
            };
        };

        Because of =  () => parsingResult = questionDataParser.TryParse(answer, questionVarName + "__1", question, out parcedValue, out parsedSingleColumnAnswer);

        It should_result_be_ParsedValueIsNotAllowed = () => parsingResult.ShouldEqual(ValueParsingResult.ParsedValueIsNotAllowed);
    }
}