using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public abstract class ActionDialogViewModel<T> : MvxViewModel<T> where T : IActionDialogArgs
{
    protected readonly IMvxNavigationService MvxNavigationService;
    protected readonly IPrincipal Principal;
    protected readonly IPlainStorage<InterviewView> InterviewStorage;
    protected readonly IPlainStorage<AssignmentDocument, int> AssignmentsStorage;
    protected readonly IPlainStorage<InterviewerDocument> UsersRepository;

    private string dialogTitle;
    private string commentHint = UIResources.Interviewer_Comment;
    private string commentHelperText;
    private string comments;
    private bool showResponsibles;
    private IMvxAsyncCommand applyCommand;
    private bool canApply;
    private string cancelTitle = UIResources.Cancel;
    private string applyTitle;
    private bool showConfirm;
    private bool isConfirmed;
    private string confirmText;

    protected T CreateParameter;

    protected ActionDialogViewModel(IMvxNavigationService mvxMvxNavigationService, 
        IPrincipal principal,
        IPlainStorage<InterviewView> interviewStorage, 
        IPlainStorage<AssignmentDocument, int> assignmentsStorage,
        IPlainStorage<InterviewerDocument> usersRepository)
    {
        this.MvxNavigationService = mvxMvxNavigationService;
        this.Principal = principal;
        this.InterviewStorage = interviewStorage;
        this.AssignmentsStorage = assignmentsStorage;
        this.UsersRepository = usersRepository;
    }

    public virtual string DialogTitle
    {
        get => dialogTitle;
        protected set => SetProperty(ref dialogTitle, value);
    }

    public virtual string CommentHint
    {
        get => commentHint;
        protected set => SetProperty(ref commentHint, value);
    }

    public virtual string ResponsiblesTitle
    {
        get => commentHelperText;
        protected set => SetProperty(ref commentHelperText, value);
    }

    public string Comments
    {
        get => comments;
        set => SetProperty(ref comments, value);
    }

    public virtual bool ShowResponsibles
    {
        get => showResponsibles;
        protected set => SetProperty(ref showResponsibles, value);
    }
    
    public virtual bool NeedAddSupervisorToResponsibles { get; protected set; }
    
    private MvxObservableCollection<ResponsibleToSelectViewModel> responsibleItems = new();
    public MvxObservableCollection<ResponsibleToSelectViewModel> ResponsibleItems
    {
        get => this.responsibleItems;
        protected set
        {
            this.SetProperty(ref this.responsibleItems, value);
            ShowResponsibles = value.Count > 0;
        }
    }

    public virtual bool CanApply
    {
        get => canApply;
        protected set => SetProperty(ref canApply, value);
    }

    public virtual string ApplyTitle
    {
        get => applyTitle;
        protected set => SetProperty(ref applyTitle, value);
    }

    public virtual string CancelTitle
    {
        get => cancelTitle;
        protected set => SetProperty(ref cancelTitle, value);
    }

    public virtual bool ShowConfirm
    {
        get => showConfirm;
        protected set => SetProperty(ref showConfirm, value);
    }

    public bool IsConfirmed
    {
        get => isConfirmed;
        set
        {
            var theSame = isConfirmed == value;
            SetProperty(ref isConfirmed, value);
            if (!theSame)
                ConfirmStateChanged(value);
        }
    }

    protected virtual void ConfirmStateChanged(bool newValue) { }

    public virtual string ConfirmText
    {
        get => confirmText;
        protected set => SetProperty(ref confirmText, value);
    }

    public IMvxAsyncCommand ApplyCommand => applyCommand ??= new MvxAsyncCommand(this.ApplyAsync, () => this.CanApply);
    protected abstract Task ApplyAsync();

    public IMvxCommand CancelCommand => new MvxCommand(this.Cancel);
    protected void Cancel() => this.MvxNavigationService.Close(this);
    
    public IMvxCommand SelectResponsibleCommand => new MvxCommand<ResponsibleToSelectViewModel>(this.SelectResponsible);

    public override void Prepare(T parameter)
    {
        this.CreateParameter = parameter;

        SetupResponsibles(parameter);
    }

    protected virtual void SetupResponsibles(T parameter)
    {
        if (!ShowResponsibles)
            return;
        
        NeedAddSupervisorToResponsibles = Principal.CurrentUserIdentity.UserId != GetCurrentEntityResponsible(parameter);
        var responsiblesViewModels = GetResponsiblesViewModels(parameter);
        this.ResponsibleItems = new MvxObservableCollection<ResponsibleToSelectViewModel>(responsiblesViewModels);
    }

    protected IEnumerable<ResponsibleToSelectViewModel> GetResponsiblesViewModels(T parameter)
    {
        var interviewers = GetInterviewers(parameter);
        var interviewerViewModels = interviewers.Select(ToInterviewerToSelectViewModel)
            .OrderBy(x => $"{x.FullName}{(string.IsNullOrEmpty(x.FullName) ? "" : " - ")}{x.Login}");

        if (!NeedAddSupervisorToResponsibles)
            return interviewerViewModels;
        
        var supervisorViewModel = GetSupervisorViewModel();
        var responsibles = supervisorViewModel.ToEnumerable()
            .Concat(interviewerViewModels)
            .ToList<ResponsibleToSelectViewModel>();

        return responsibles;
    }
    
    protected virtual Guid? GetCurrentEntityResponsible(T parameter) => null;

    private ResponsibleToSelectViewModel GetSupervisorViewModel()
    {
        var userIdentity = Principal.CurrentUserIdentity;
            
        var interviewsQuantityByInterviewer = this.AssignmentsStorage.WhereSelect(
            @where => @where.ResponsibleId == userIdentity.UserId,
            @select =>
                new InterviewsQuantity
                {
                    Quantity = @select.Quantity,
                    CreatedInterviewsCount = @select.CreatedInterviewsCount
                });

        var assignmentsCount = interviewsQuantityByInterviewer.Sum(x =>
            x.Quantity == null ? 1 : x.Quantity.GetValueOrDefault() - x.CreatedInterviewsCount.GetValueOrDefault());

        var interviewsCount = this.InterviewStorage.Count(y => y.ResponsibleId == userIdentity.UserId);
            
        return new ResponsibleToSelectViewModel(option => this.SelectResponsibleCommand.Execute(option))
        {
            Id = userIdentity.UserId,
            Login = userIdentity.Name,
            Role = UserRoles.Supervisor,
            InterviewsCount = interviewsCount + assignmentsCount,
        };
    }
    
    protected virtual IEnumerable<InterviewerDocument> GetInterviewers(T parameter)
    {
        var responsibleId = GetCurrentEntityResponsible(parameter);

        var interviewersViewModels = this.UsersRepository.LoadAll()
            .Where(x => x.InterviewerId != responsibleId
                        && !x.IsLockedByHeadquarters
                        && !x.IsLockedBySupervisor);
            
        return interviewersViewModels;
    }

    private ResponsibleToSelectViewModel ToInterviewerToSelectViewModel(InterviewerDocument interviewer)
    {
        var interviewsQuantityByInterviewer = this.AssignmentsStorage.WhereSelect(
            @where => @where.ResponsibleId == interviewer.InterviewerId,
            @select =>
                new InterviewsQuantity
                {
                    Quantity = @select.Quantity,
                    CreatedInterviewsCount = @select.CreatedInterviewsCount
                });

        var assignmentsCount = interviewsQuantityByInterviewer.Sum(x =>
            x.Quantity == null ? 1 : x.Quantity.GetValueOrDefault() - x.CreatedInterviewsCount.GetValueOrDefault());

        var interviewsCount = this.InterviewStorage.Count(y =>
            y.ResponsibleId == interviewer.InterviewerId && 
            (y.Status == InterviewStatus.RejectedBySupervisor || y.Status == InterviewStatus.RejectedByHeadquarters));

        return new ResponsibleToSelectViewModel(option => this.SelectResponsibleCommand.Execute(option))
        {
            Id = interviewer.InterviewerId,
            Login = interviewer.UserName,
            FullName = interviewer.FullaName,
            InterviewsCount = interviewsCount + assignmentsCount,
            Role = UserRoles.Interviewer,
        };
    }
    
    private void SelectResponsible(ResponsibleToSelectViewModel responsible)
    {
        this.ResponsibleItems
            .Where(x => x != responsible)
            .ForEach(x => x.IsSelected = false);

        ResponsibleSelected(responsible);
    }

    protected virtual void ResponsibleSelected(ResponsibleToSelectViewModel responsible) { }

    private class InterviewsQuantity
    {
        public int? Quantity { get; set; }
        public int? CreatedInterviewsCount { get; set; }
    }
}