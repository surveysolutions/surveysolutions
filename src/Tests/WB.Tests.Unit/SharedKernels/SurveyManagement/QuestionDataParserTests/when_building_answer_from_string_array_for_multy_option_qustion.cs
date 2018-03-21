using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NHibernate.Util;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_building_answer_from_string_array_for_multy_option_qustion : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDataParser = CreateQuestionDataParser();
            question = new MultyOptionsQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.MultyOption,
                Answers =
                    new List<Answer>()
                    {
                        new Answer() {AnswerValue = "1", AnswerText = "a"},
                        new Answer() {AnswerValue = "2", AnswerText = "b"},
                        new Answer() {AnswerValue = "3", AnswerText = "c"}
                    },
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        private void BecauseOf() =>
                result =
                    questionDataParser.BuildAnswerFromStringArray(new [] { new Tuple<string, string>(questionVarName + "__1", "1"), new Tuple<string, string>(questionVarName + "__2", "2") },
                        question);

        [NUnit.Framework.Test] public void should_result_be_type_of_array_of_decimal () =>
            result.Should().BeOfType<CategoricalFixedMultiOptionAnswer>();

        [NUnit.Framework.Test] public void should_result_has_2_answers () =>
            ((CategoricalFixedMultiOptionAnswer) result).CheckedValues.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_result_first_item_equal_to_1 () =>
            ((CategoricalFixedMultiOptionAnswer) result).CheckedValues.First().Should().Be(1);

        [NUnit.Framework.Test] public void should_result_second_item_equal_to_2 () =>
            ((CategoricalFixedMultiOptionAnswer) result).CheckedValues.Last().Should().Be(2);
    }
}
