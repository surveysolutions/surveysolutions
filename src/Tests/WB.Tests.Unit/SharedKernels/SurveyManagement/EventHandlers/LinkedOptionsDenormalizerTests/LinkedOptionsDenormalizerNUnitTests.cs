using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

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
            var linkedQuestionIdentity = Create.Entity.Identity(linkedQuestionId, RosterVector.Empty);
            var changedLinkedOptions = new ChangedLinkedOptions(linkedQuestionIdentity,
                new[]
                {
                    Create.Entity.RosterVector(1),
                    Create.Entity.RosterVector(2)
                });
            var updatedInterviewLinkedQuestionOptions = linkedOptionsDenormalizer.Update(new InterviewLinkedQuestionOptions(),
                Create.Event.LinkedOptionsChanged(new[]{changedLinkedOptions}).ToPublishedEvent());

            Assert.That(updatedInterviewLinkedQuestionOptions.LinkedQuestionOptions[linkedQuestionIdentity.ToString()],
                Is.EqualTo(changedLinkedOptions.Options));
        }

        private LinkedOptionsDenormalizer CreateLinkedOptionsDenormalizer() { return new LinkedOptionsDenormalizer(new TestInMemoryWriter<InterviewLinkedQuestionOptions>());}
    }
}