using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.LinkedOptionsDenormalizerTests
{
    [TestFixture]
    internal class LinkedOptionsDenormalizerNUnitTests
    {
        [Test]
        public void
            Handle_When_LinkedOptionsChanged_event_arrived_Then_updated_version_of_should_be_returned
            ()
        {
            LinkedOptionsDenormalizer linkedOptionsDenormalizer = CreateLinkedOptionsDenormalizer();
            var linkedQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var linkedQuestionIdentity = Create.Identity(linkedQuestionId, RosterVector.Empty);
            var changedLinkedOptions = new ChangedLinkedOptions(linkedQuestionIdentity,
                new[]
                {
                    Create.RosterVector(1),
                    Create.RosterVector(2)
                });
            var updatedInterviewLinkedQuestionOptions = linkedOptionsDenormalizer.Update(new InterviewLinkedQuestionOptions(),
                Create.Event.LinkedOptionsChanged(new[]{changedLinkedOptions}).ToPublishedEvent());

            Assert.That(updatedInterviewLinkedQuestionOptions.LinkedQuestionOptions[linkedQuestionIdentity.ToString()],
                Is.EqualTo(changedLinkedOptions.Options));
        }

        private LinkedOptionsDenormalizer CreateLinkedOptionsDenormalizer() { return new LinkedOptionsDenormalizer(new TestInMemoryWriter<InterviewLinkedQuestionOptions>());}
    }
}