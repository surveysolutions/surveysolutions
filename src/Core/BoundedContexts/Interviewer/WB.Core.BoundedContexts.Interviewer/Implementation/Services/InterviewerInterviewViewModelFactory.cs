using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerInterviewViewModelFactory : InterviewViewModelFactory
    {
        private readonly IServiceLocator serviceLocator;

        public InterviewerInterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository, 
            IStatefulInterviewRepository interviewRepository,
            IEnumeratorSettings settings,
            IServiceLocator serviceLocator) : base(questionnaireRepository, interviewRepository, settings)
        {
            this.serviceLocator = serviceLocator;
        }

        public override IDashboardItem GetDashboardAssignment(AssignmentDocument assignment)
        {
            InterviewerAssignmentDashboardItemViewModel result =
                serviceLocator.GetInstance<InterviewerAssignmentDashboardItemViewModel>();
            result.Init(assignment);
            return result;
        }
    }
}
