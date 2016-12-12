using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_numeric_qustion : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "1";
            questionDataParser = CreateQuestionDataParser();
            question = new NumericQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Numeric,
                IsInteger = true,
                StataExportCaption = questionVarName
            };
        };

        Because of =
            () =>
                result =
                    questionDataParser.BuildAnswerFromStringArray(new[] { new Tuple<string, string>(questionVarName, answer) }, question);

        It should_result_be_equal_to_1 = () =>
            (result as NumericIntegerAnswer)?.Value.ShouldEqual(1);
    }
}