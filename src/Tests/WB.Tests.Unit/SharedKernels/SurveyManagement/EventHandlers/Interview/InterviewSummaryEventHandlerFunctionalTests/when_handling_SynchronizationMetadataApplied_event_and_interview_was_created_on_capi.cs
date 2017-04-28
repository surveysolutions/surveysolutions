using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_SynchronizationMetadataApplied_event_and_interview_was_created_on_capi : InterviewSummaryDenormalizerTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary()
            {
                WasCreatedOnClient = true,
                ResponsibleId = responsibleId
            };

            var usersMock = new Mock<IUserViewFactory>();

            var responsibleStringId = responsibleId.FormatGuid();
            usersMock.Setup(_ => _.GetUser(Moq.It.Is<UserViewInputModel>(x=>x.PublicKey == responsibleId)))
                .Returns(new UserView() {Supervisor = new UserLight() {Id = supervisorId, Name = supervisorName}});

            denormalizer = CreateDenormalizer(users: usersMock.Object);
        };

        Because of = () =>
            viewModel =
                denormalizer.Update(viewModel,
                    Create.PublishedEvent.SynchronizationMetadataApplied(status: interviewStatus));

        It should_teamLeadId_be_equal_to_specified_supervisorId = () =>
            viewModel.TeamLeadId.ShouldEqual(supervisorId);

        It should_teamLeadName_be_equal_to_specified_supervisorName = () =>
            viewModel.TeamLeadName.ShouldEqual(supervisorName);

        It should_status_be_equal_specified_interviewStatus = () =>
            viewModel.Status.ShouldEqual(interviewStatus);

        private static InterviewSummaryDenormalizer denormalizer;
        private static InterviewSummary viewModel;
        private static Guid responsibleId = Guid.Parse("11111111111111111111111111111111");
        private static Guid supervisorId = Guid.Parse("22222222222222222222222222222222");
        private static string supervisorName = "supervisor";
        private static InterviewStatus interviewStatus = InterviewStatus.ApprovedByHeadquarters;
    }
}
