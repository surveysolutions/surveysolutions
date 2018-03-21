using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_yesno_qustion : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDataParser = CreateQuestionDataParser();
            question = new MultyOptionsQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.MultyOption,
                YesNoView = true,
                Answers = new List<Answer>
                {
                    new Answer {AnswerValue = "1", AnswerText = "a"},
                    new Answer {AnswerValue = "2", AnswerText = "b"},
                    new Answer {AnswerValue = "3", AnswerText = "c"}
                },
                StataExportCaption = questionVarName
            };

            answerWithColumns = new[]
            {
                Tuple.Create(questionVarName + "__1", "1"),
                Tuple.Create(questionVarName + "__1", ""),
                Tuple.Create(questionVarName + "__3", "0")
            };
            BecauseOf();
        }
        
        public void BecauseOf() => result = questionDataParser.BuildAnswerFromStringArray(answerWithColumns, question);

        [NUnit.Framework.Test] public void should_return_array_of_yesno_answers () =>
            result.Should().BeOfType<YesNoAnswer>();

        [NUnit.Framework.Test] public void should_set_yes_to_yes_the_option () => ((YesNoAnswer)result).CheckedOptions.First().Yes.Should().BeTrue();

        [NUnit.Framework.Test] public void should_set_no_to_no_option () => ((YesNoAnswer)result).CheckedOptions.Last().Yes.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_add_not_answered_option () => ((YesNoAnswer)result).CheckedOptions.Count.Should().Be(2);

        static Tuple<string, string>[] answerWithColumns;
    }
}
