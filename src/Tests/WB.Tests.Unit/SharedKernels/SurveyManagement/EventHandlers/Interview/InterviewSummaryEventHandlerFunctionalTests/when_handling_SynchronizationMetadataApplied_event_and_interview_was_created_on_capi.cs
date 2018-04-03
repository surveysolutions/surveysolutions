using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_SynchronizationMetadataApplied_event_and_interview_was_created_on_capi : InterviewSummaryDenormalizerTestsContext
    {
        [Test]
        public void should_update_team_lead_properies()
        {
            var viewModel = new InterviewSummary()
            {
                WasCreatedOnClient = true,
                ResponsibleId = responsibleId
            };

            var usersMock = new Mock<IUserViewFactory>();

            usersMock.Setup(_ => _.GetUser(Moq.It.Is<UserViewInputModel>(x=>x.PublicKey == responsibleId)))
                .Returns(new UserView() {Supervisor = new UserLight() {Id = supervisorId, Name = supervisorName}});

            var denormalizer = CreateDenormalizer(users: usersMock.Object);

            // Act
            viewModel =
                denormalizer.Update(viewModel,
                    Create.PublishedEvent.SynchronizationMetadataApplied(status: interviewStatus));

            // Assert
            viewModel.TeamLeadId.Should().Be(supervisorId);
            viewModel.TeamLeadName.Should().Be(supervisorName);
            viewModel.Status.Should().Be(interviewStatus);
        }
         
        private static Guid responsibleId = Id.g1;
        private static Guid supervisorId = Id.g2;
        private static string supervisorName = "supervisor";
        private static InterviewStatus interviewStatus = InterviewStatus.ApprovedByHeadquarters;
    }
}
