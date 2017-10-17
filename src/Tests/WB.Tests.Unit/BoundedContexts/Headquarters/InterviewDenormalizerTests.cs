using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Headquarters
{
    [TestOf(typeof(InterviewDenormalizer))]
    internal class InterviewDenormalizerTests
    {
        [Test]
        public void when_remove_rosters_should_be_questionnaire_identity_getting_from_cache()
        {
            //arrange
            var interviewId = Guid.Parse("22222222222222222222222222222222");
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionnaireVersion = 1515L;

            var mockOfInterviewFactory = new Mock<IInterviewFactory>();
            var interviewDenormalizer = new InterviewDenormalizer(mockOfInterviewFactory.Object);

            interviewDenormalizer.Handle(Create.PublishedEvent.InterviewCreated(interviewId: interviewId,
                questionnaireId: questionnaireId.ToString(), questionnaireVersion: questionnaireVersion));

            //act
            interviewDenormalizer.Handle(Create.PublishedEvent.RosterInstancesRemoved(interviewId));

            //assert
            mockOfInterviewFactory.Verify(x => x.RemoveRosters(
                Moq.It.Is<QuestionnaireIdentity>(y => y.QuestionnaireId == questionnaireId &&
                                                      y.Version == questionnaireVersion), interviewId,
                Moq.It.IsAny<Identity[]>()), Times.Once);
        }
    }
}
