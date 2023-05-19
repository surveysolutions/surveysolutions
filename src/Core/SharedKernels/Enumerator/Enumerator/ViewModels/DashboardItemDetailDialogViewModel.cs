#nullable enable

using System;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public class DashboardItemDetailDialogViewModel: MvxViewModel<DashboardItemDetailDialogViewModelArgs>
{
    private readonly IMvxNavigationService navigationService;
    private readonly IAssignmentDocumentsStorage assignmentsRepository;
    private readonly IPlainStorage<InterviewView> interviewViewRepository;
    private readonly IDashboardViewModelFactory dashboardViewModelFactory;

    private DashboardItemDetailDialogViewModelArgs? initValues;
    private IDashboardItem? dashboardViewItem = new DashboardSubTitleViewModel();

    public DashboardItemDetailDialogViewModel(
        IDashboardViewModelFactory dashboardViewModelFactory,
        IMvxNavigationService navigationService,
        IAssignmentDocumentsStorage assignmentsRepository,
        IPlainStorage<InterviewView> interviewViewRepository)
    {
        this.navigationService = navigationService;
        this.assignmentsRepository = assignmentsRepository;
        this.interviewViewRepository = interviewViewRepository;
        this.dashboardViewModelFactory = dashboardViewModelFactory;
    }

    public override void Prepare(DashboardItemDetailDialogViewModelArgs param)
    {
        base.Prepare();

        initValues = param;

        if (param.InterviewId.HasValue)
        {
            var interviewView = interviewViewRepository.GetById(param.InterviewId.Value.FormatGuid());
            DashboardViewItem = dashboardViewModelFactory.GetInterview(interviewView);
        }
        else if (param.AssignmentId.HasValue)
        {
            var assignmentDocument = assignmentsRepository.GetById(param.AssignmentId.Value);
            DashboardViewItem = dashboardViewModelFactory.GetAssignment(assignmentDocument);
        }

        if (dashboardViewItem == null)
            throw new NullReferenceException("dashboardViewItem can't be null");

        dashboardViewItem.IsExpanded = true;
        //  Title = 
    }

    public string? Title { get; set; }

    public IDashboardItem? DashboardViewItem
    {
        get => dashboardViewItem;
        set => SetProperty(ref dashboardViewItem, value);
    }

    public IMvxAsyncCommand CloseCommand =>
        new MvxAsyncCommand(async () => await this.navigationService.Close(this));
}