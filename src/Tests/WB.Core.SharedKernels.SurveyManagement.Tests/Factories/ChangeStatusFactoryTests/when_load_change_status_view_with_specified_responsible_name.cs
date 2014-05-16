using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.Factories.ChangeStatusFactoryTests
{
    internal class when_load_change_status_view_with_specified_responsible_name : ChangeStatusFactoryTestsContext
    {
        Establish context = () =>
        {
            var interviewsMock = new Mock<IQueryableReadSideRepositoryReader<InterviewSummary>>();
            interviewsMock.Setup(_ => _.GetById(Moq.It.IsAny<string>()))
                .Returns(new InterviewSummary() {ResponsibleName = responsibleName});

            factory = CreateFactory(interviewsMock.Object);
        };

        Because of = () =>
            viewModel = factory.Load(new ChangeStatusInputModel());

        It should_view_model_responsible_be_equal_to_responsibleName = () =>
            viewModel.Responsible.ShouldEqual(responsibleName);

        private static ChangeStatusFactory factory;
        private static ChangeStatusView viewModel;
        private static string responsibleName;
    }
}
