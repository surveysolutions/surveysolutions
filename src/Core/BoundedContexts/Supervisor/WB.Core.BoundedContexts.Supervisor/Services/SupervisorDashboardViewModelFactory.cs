using System.Linq;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Items;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.Services;

public class SupervisorDashboardViewModelFactory : IDashboardViewModelFactory
{
    private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;
    private readonly IInterviewViewModelFactory viewModelFactory;

    public SupervisorDashboardViewModelFactory(IInterviewViewModelFactory viewModelFactory,
        IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo)
    {
        this.identifyingQuestionsRepo = identifyingQuestionsRepo;
        this.viewModelFactory = viewModelFactory;
    }

    public InterviewDashboardItemViewModel GetInterview(InterviewView interview)
    {
        var prefilledQuestions = this.identifyingQuestionsRepo
            .Where(x => x.InterviewId == interview.InterviewId)
            .OrderBy(x => x.SortIndex)
            .Select(fi => new PrefilledQuestion {Answer = fi.Answer?.Trim(), Question = fi.QuestionText})
            .ToList();

        var dashboardItem = this.viewModelFactory.GetNew<SupervisorInterviewDashboardViewModel>();
        dashboardItem.Init(interview, prefilledQuestions);
        return dashboardItem;
    }

    public AssignmentDashboardItemViewModel GetAssignment(AssignmentDocument assignment)
    {
        var viewModel = viewModelFactory.GetNew<SupervisorAssignmentDashboardItemViewModel>();
        viewModel.Init(assignment);
        return viewModel;
    }
}