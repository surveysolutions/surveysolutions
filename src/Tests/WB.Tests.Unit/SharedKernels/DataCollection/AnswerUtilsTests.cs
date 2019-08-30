﻿using System.Collections.Generic;
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
    }
}
