using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewEntities
{
    public class AnswersSerializationTests
    {
        NewtonInterviewAnswerJsonSerializer Serializer = new NewtonInterviewAnswerJsonSerializer();

        [Test]
        public void should_be_able_to_preserve_answers_during_serialization_of_single_option_answer()
        {
            CategoricalFixedSingleOptionAnswer sgl = CategoricalFixedSingleOptionAnswer.FromInt(1);

            var serialized = Serializer.Serialize(sgl);
            var deserialized = Serializer.Deserialize<CategoricalFixedSingleOptionAnswer>(serialized);

            Assert.That(deserialized, Has.Property(nameof(deserialized.SelectedValue)).EqualTo(sgl.SelectedValue));
        }

        [Test]
        public void should_be_able_to_deserialize_existing_answer_from_db_that_was_written_prior_to_19_06_release()
        {
            string serialized =
                ResourceHelper.ReadResourceFile(
                    "WB.Tests.Unit.SharedKernels.DataCollection.InterviewEntities.serializedAnswers.json");

            var deserialized = Serializer.Deserialize<IList<InterviewAnswer>>(serialized);

            var toCheck = (CategoricalFixedSingleOptionAnswer) deserialized[0].Answer;
            Assert.That(toCheck, Has.Property(nameof(toCheck.SelectedValue)).EqualTo(1000));
            var identity = Create.Identity(Guid.Parse("3c8097f6-fdc3-d63f-ddcc-75cff8062f28"), 1, 2);
            Assert.That(deserialized[0].Identity, Is.EqualTo(identity));
        }
    }
}
