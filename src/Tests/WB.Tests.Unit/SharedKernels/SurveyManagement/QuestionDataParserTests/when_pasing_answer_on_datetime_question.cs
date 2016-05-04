using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_datetime_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = new DateTimeQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = questionVarName
            };
        };

        Because of =() =>
            parsingResult = questionDataParser.TryParse(expectedDateTimeAnswer.ToString(@"MM\/dd\/yyyy HH:mm tt"), questionVarName, question, out parcedValue);

        It should_result_be_equal_to_expectedDateTimeAnswer = () =>
            parcedValue.ShouldEqual(expectedDateTimeAnswer);

        private static readonly DateTime expectedDateTimeAnswer = new DateTime(2016, 02, 13, 17, 04, 00);
    }
}