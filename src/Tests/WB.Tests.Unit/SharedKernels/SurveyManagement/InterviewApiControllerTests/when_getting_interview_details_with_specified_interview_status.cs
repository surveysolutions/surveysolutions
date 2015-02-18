using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewApiControllerTests
{
    internal class when_getting_interview_details_with_specified_interview_status : InterviewApiControllerTestsContext
    {
        private Establish context = () =>
        {
            var interviewDetailsFactoryMock =
                new Mock<IInterviewDetailsViewFactory>();
            interviewDetailsFactoryMock.Setup(_ => _.GetInterviewDetails(Moq.It.IsAny<Guid>(), Moq.It.IsAny<Guid?>(), Moq.It.IsAny<decimal[]>(), Moq.It.IsAny<InterviewDetailsFilter?>()))
                .Returns(
                    () => new DetailsViewModel()
                    {
                        InterviewDetails = new InterviewDetailsView()
                        {
                            Status = verifiedStatus,
                            Responsible = new UserLight(new Guid(), "some user name")
                        }
                    });

            var globalInfoProvider = Mock.Of<IGlobalInfoProvider>(_
                => _.IsHeadquarter == true);

            controller = CreateController(
                interviewDetailsFactory: interviewDetailsFactoryMock.Object,
                globalInfoProvider: globalInfoProvider);
        };

        Because of = () =>
            viewModel = controller.InterviewDetails(new InterviewDetailsViewModel {InterviewId = new Guid()});

        It should_view_model_contains_localized_interview_status_from_resources = () =>
            viewModel.InterviewInfo.status.ShouldEqual(verifiedStatus.ToLocalizeString());

        private static InterviewApiController controller;
        private static NewInterviewDetailsView viewModel;
        private static InterviewStatus verifiedStatus = InterviewStatus.SupervisorAssigned;
    }
}
