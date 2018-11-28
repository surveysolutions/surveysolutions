using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    public class TesterInterviewViewModelFactory : InterviewViewModelFactory
    {
        public TesterInterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository, 
            IStatefulInterviewRepository interviewRepository, 
            IEnumeratorSettings settings, IServiceLocator serviceLocator) : base(questionnaireRepository, interviewRepository, settings, serviceLocator)
        {
        }

        public override IDashboardItem GetDashboardAssignment(AssignmentDocument assignment)
        {
            throw new System.NotSupportedException("Tester does not have assignments");
        }
    }
}
