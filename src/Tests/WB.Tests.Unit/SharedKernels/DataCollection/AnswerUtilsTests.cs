using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection
{
    [TestOf(typeof(AnswerUtils))]
    public class AnswerUtilsTests
    {
        [Test]
        public void When_Getting_options_from_categorical_question_in_mixed_format_and_parent_value_is_not_set()
        {
            var question = Create.Entity.SingleQuestion(options: new List<Answer>
            {
                Create.Entity.Option(1, "Hello 1", null),
                Create.Entity.Option(2, "Hello 2", null),
                Create.Entity.Option(3, "Hello 3", "1"),
                Create.Entity.Option(4, "Hello 4", "2"),
            });

            var options = AnswerUtils.GetCategoricalOptionsFromQuestion(question, null, null).ToArray();

            Assert.That(options.Length, Is.EqualTo(4));
        }
        
        [Test]
        public void When_Getting_options_from_categorical_question_in_old_format_and_parent_value()
        {
            var question = Create.Entity.SingleQuestion(options: new List<Answer>
            {
                Create.Entity.Option("1", "Hello 1", "1"),
                Create.Entity.Option("2", "Hello 2", "1"),
                Create.Entity.Option("3", "Hello 3", "2"),
                Create.Entity.Option("4", "Hello 4", "2"),
            });

            var options = AnswerUtils.GetCategoricalOptionsFromQuestion(question, null, null).ToArray();

            Assert.That(options.Length, Is.EqualTo(4));
        }

        [Test]
        public void When_Getting_options_from_categorical_question_in_new_format_and_parent_value()
        {
            var question = Create.Entity.SingleQuestion(options: new List<Answer>
            {
                Create.Entity.Option(1, "Hello 1", "1"),
                Create.Entity.Option(2, "Hello 2", "1"),
                Create.Entity.Option(3, "Hello 3", "2"),
                Create.Entity.Option(4, "Hello 4", "2"),
            });

            var options = AnswerUtils.GetCategoricalOptionsFromQuestion(question, null, null).ToArray();

            Assert.That(options.Length, Is.EqualTo(4));
        }

        [Test]
        public void When_Getting_options_from_categorical_question_with_parent_value_should_return_options_filtered_by_parent_value()
        {
            var question = Create.Entity.SingleQuestion(options: new List<Answer>
            {
                Create.Entity.OptionByCode(1, "Hello 1", 1),
                Create.Entity.OptionByCode(2, "Hello 2", 1),
                Create.Entity.OptionByCode(3, "Hello 3", 2),
                Create.Entity.OptionByCode(4, "Hello 4", 2),
            });

            var options = AnswerUtils.GetCategoricalOptionsFromQuestion(question, 1, null).ToArray();

            Assert.That(options.Length, Is.EqualTo(2));
            Assert.That(options.Select(x => x.Value), Is.EquivalentTo(new[] {1, 2}));
        }

        [Test]
        public void When_GetOptionForQuestionByOptionValue_for_cascading_question_then_should_return_option_by_value_and_parent_value()
        {
            var expectedOption = Create.Entity.OptionByCode(2, "Hello 2", 1);
            var question = Create.Entity.SingleQuestion(options: new List<Answer>
            {
                Create.Entity.OptionByCode(1, "Hello 1", 1),
                expectedOption,
                Create.Entity.OptionByCode(3, "Hello 3", 2),
                Create.Entity.OptionByCode(4, "Hello 4", 2),
            }, cascadeFromQuestionId: Guid.NewGuid());

            var option = AnswerUtils.GetOptionForQuestionByOptionValue(question, 2, 1);

            Assert.That(option, Is.Not.Null);
            Assert.That(option.Title, Is.EqualTo(expectedOption.AnswerText));
            Assert.That(option.Value, Is.EqualTo(expectedOption.AnswerCode));
            Assert.That(option.ParentValue, Is.EqualTo(expectedOption.ParentCode));
        }

        [Test]
        public void When_GetOptionForQuestionByOptionValue_for_non_cascading_question_then_should_return_option_by_value_and_ignore_parent_value()
        {
            var expectedOption = Create.Entity.OptionByCode(2, "Hello 2", 1);
            var question = Create.Entity.SingleQuestion(options: new List<Answer>
            {
                Create.Entity.OptionByCode(1, "Hello 1", 1),
                expectedOption,
                Create.Entity.OptionByCode(3, "Hello 3", 2),
                Create.Entity.OptionByCode(4, "Hello 4", 2),
            });

            var option = AnswerUtils.GetOptionForQuestionByOptionValue(question, 2, null);

            Assert.That(option, Is.Not.Null);
            Assert.That(option.Title, Is.EqualTo(expectedOption.AnswerText));
            Assert.That(option.Value, Is.EqualTo(expectedOption.AnswerCode));
            Assert.That(option.ParentValue, Is.EqualTo(expectedOption.ParentCode));
        }

        [Test]
        public void when_AnswerToString_for_big_decimal_value_should_return_regular_format_string()
        {
            decimal number1 = 12345678901234567890m;
            Assert.That(AnswerUtils.AnswerToString(number1), Is.EqualTo("12345678901234567890"));

            decimal number2 = 12345678901234567890123456789m;
            Assert.That(AnswerUtils.AnswerToString(number2), Is.EqualTo("12345678901234567890123456789"));

            decimal number3 = 12345678901234567890.123456789m;
            Assert.That(AnswerUtils.AnswerToString(number3), Is.EqualTo("12345678901234567890.123456789"));

            decimal number4 = 0.1234567890123456789012345678m;
            Assert.That(AnswerUtils.AnswerToString(number4), Is.EqualTo("0.1234567890123456789012345678"));
        }
    }
}
