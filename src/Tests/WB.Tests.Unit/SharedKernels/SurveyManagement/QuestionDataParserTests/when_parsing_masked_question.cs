using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_parsing_masked_question_with_valid_answer : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDataParser = CreateQuestionDataParser();
            question = new TextQuestion
            {
                Mask = "##",
                PublicKey = questionId,
                QuestionType = QuestionType.Text,
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        private void BecauseOf() =>
              parseResult =
                  questionDataParser.TryParse("22",questionVarName,
                      question, out parsedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_return_OK () => parseResult.Should().Be(ValueParsingResult.OK);

        private static ValueParsingResult parseResult;
        private static object parsedValue;
    }
}

