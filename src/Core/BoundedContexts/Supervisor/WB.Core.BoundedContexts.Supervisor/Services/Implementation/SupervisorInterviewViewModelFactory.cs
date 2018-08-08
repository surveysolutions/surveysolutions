using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorInterviewViewModelFactory : InterviewViewModelFactory
    {
        public SupervisorInterviewViewModelFactory(IQuestionnaireStorage questionnaireRepository, IStatefulInterviewRepository interviewRepository, IEnumeratorSettings settings) : base(questionnaireRepository, interviewRepository, settings)
        {
        }

        public override IReadOnlyList<Guid> GetUnderlyingInterviewerEntities(Identity groupIdentity, IQuestionnaire questionnaire)
        {
            return questionnaire.GetChildEntityIds(groupIdentity.Id).ToReadOnlyCollection();
        }

        public override IDashboardItem GetDashboardAssignment(AssignmentDocument assignment)
        {
            SupervisorAssignmentDashboardItemViewModel result =
                ServiceLocator.Current.GetInstance<SupervisorAssignmentDashboardItemViewModel>();
            result.Init(assignment);
            return result;
        }
    }
}
