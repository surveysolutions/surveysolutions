using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    internal class when_handling_SynchronizationMetadataApplied_event_and_interview_was_created_on_capi : InterviewSummaryEventHandlerFunctionalTestsContext
    {
        private Establish context = () =>
        {
            viewModel = new InterviewSummary()
            {
                WasCreatedOnClient = true,
                ResponsibleId = responsibleId
            };

            var usersMock = new Mock<IReadSideRepositoryWriter<UserDocument>>();

            usersMock.Setup(_ => _.GetById(responsibleId.FormatGuid()))
                .Returns(new UserDocument() {Supervisor = new UserLight() {Id = supervisorId, Name = supervisorName}});

            denormalizer = CreateDenormalizer(users: usersMock.Object);
        };

        Because of = () =>
            viewModel =
                denormalizer.Update(viewModel,
                    Create.SynchronizationMetadataAppliedEvent(status: interviewStatus));

        It should_teamLeadId_be_equal_to_specified_supervisorId = () =>
            viewModel.TeamLeadId.ShouldEqual(supervisorId);

        It should_teamLeadName_be_equal_to_specified_supervisorName = () =>
            viewModel.TeamLeadName.ShouldEqual(supervisorName);

        It should_status_be_equal_specified_interviewStatus = () =>
            viewModel.Status.ShouldEqual(interviewStatus);

        private static InterviewSummaryEventHandlerFunctional denormalizer;
        private static InterviewSummary viewModel;
        private static Guid responsibleId = Guid.Parse("11111111111111111111111111111111");
        private static Guid supervisorId = Guid.Parse("22222222222222222222222222222222");
        private static string supervisorName = "supervisor";
        private static InterviewStatus interviewStatus = InterviewStatus.ApprovedByHeadquarters;
    }
}
