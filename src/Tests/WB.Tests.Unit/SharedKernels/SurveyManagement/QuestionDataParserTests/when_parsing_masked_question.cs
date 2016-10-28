using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_parsing_masked_question_with_valid_answer : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            questionDataParser = CreateQuestionDataParser();
            question = new TextQuestion
            {
                Mask = "##",
                PublicKey = questionId,
                QuestionType = QuestionType.Text,
                StataExportCaption = questionVarName
            };
        };

        Because of =
          () =>
              parseResult =
                  questionDataParser.TryParse("22",questionVarName,
                      question, out parsedValue, out parsedSingleColumnAnswer);

        It should_return_OK = () => parseResult.ShouldEqual(ValueParsingResult.OK);

        private static ValueParsingResult parseResult;
        private static object parsedValue;
    }
}

