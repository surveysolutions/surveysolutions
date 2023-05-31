using System.Linq;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.BoundedContexts.Interviewer.Services;

public class InterviewerDashboardViewModelFactory : IDashboardViewModelFactory
{
    private readonly IInterviewViewModelFactory viewModelFactory;
    private readonly IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo;

    public InterviewerDashboardViewModelFactory(IInterviewViewModelFactory viewModelFactory,
        IPlainStorage<PrefilledQuestionView> identifyingQuestionsRepo)
    {
        this.viewModelFactory = viewModelFactory;
        this.identifyingQuestionsRepo = identifyingQuestionsRepo;
    }

    public InterviewDashboardItemViewModel GetInterview(InterviewView interview)
    {
        var details = this.identifyingQuestionsRepo
            .Where(p => p.InterviewId == interview.InterviewId)
            .OrderBy(x => x.SortIndex)
            .Select(fi => new PrefilledQuestion {Answer = fi.Answer?.Trim(), Question = fi.QuestionText})
            .ToList();
        
        var viewModel = viewModelFactory.GetNew<InterviewDashboardItemViewModel>();
        viewModel.Init(interview, details);
        return viewModel;
    }

    public AssignmentDashboardItemViewModel GetAssignment(AssignmentDocument assignment)
    {
        var viewModel = viewModelFactory.GetNew<InterviewerAssignmentDashboardItemViewModel>();
        viewModel.Init(assignment);
        return viewModel;
    }
}