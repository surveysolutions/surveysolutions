using System;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class SelectResponsibleForAssignmentViewModel : MvxViewModel<SelectResponsibleForAssignmentArgs>
    {
        private readonly IMvxNavigationService mvxNavigationService;
        private readonly IPlainStorage<InterviewerDocument> usersRepository;
        private readonly IPrincipal principal;
        private readonly IAuditLogService auditLogService;
        private readonly ICommandService commandService;
        private readonly IViewModelNavigationService navigationService;
        private readonly IMvxMessenger messenger;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IPlainStorage<InterviewView> interviewStorage;
        private readonly IPlainStorage<AssignmentDocument, int> assignmentsStorage;

        public SelectResponsibleForAssignmentViewModel(
            IPlainStorage<InterviewerDocument> usersRepository,
            IPrincipal principal,
            IAuditLogService auditLogService,
            ICommandService commandService,
            IViewModelNavigationService navigationService,
            IStatefulInterviewRepository statefulInterviewRepository,
            IPlainStorage<InterviewView> interviewStorage,
            IPlainStorage<AssignmentDocument, int> assignmentsStorage)
        {
            this.mvxNavigationService = Mvx.IoCProvider.Resolve<IMvxNavigationService>();
            this.usersRepository = usersRepository;
            this.principal = principal;
            this.auditLogService = auditLogService;
            this.commandService = commandService;
            this.navigationService = navigationService;
            this.messenger = Mvx.IoCProvider.GetSingleton<IMvxMessenger>();
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.interviewStorage = interviewStorage;
            this.assignmentsStorage = assignmentsStorage;
        }

        private bool canReassign;
        public bool CanReassign
        {
            get => this.canReassign;
            protected set => this.RaiseAndSetIfChanged(ref this.canReassign, value);
        }

        private string comments;
        public string Comments
        {
            get => this.comments;
            protected set => this.RaiseAndSetIfChanged(ref this.comments, value);
        }

        private IMvxAsyncCommand reassignCommand;
        public IMvxAsyncCommand ReassignCommand => reassignCommand ??= new MvxAsyncCommand(this.ReassignAsync, () => this.CanReassign);
        public IMvxCommand CancelCommand => new MvxCommand(this.Cancel);
        public IMvxCommand SelectResponsibleCommand => new MvxCommand<ResponsibleToSelectViewModel>(this.SelectResponsible);

        private MvxObservableCollection<ResponsibleToSelectViewModel> uiItems;
        public MvxObservableCollection<ResponsibleToSelectViewModel> UiItems
        {
            get => this.uiItems;
            private set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        private void Cancel() => this.mvxNavigationService.Close(this);

        private async Task ReassignAsync()
        {
            if (!this.CanReassign) return;
            this.CanReassign = false;

            var selectedResponsible = this.uiItems.Single(x => x.IsSelected);

            try
            {
                if (this.input.InterviewId.HasValue)
                    await this.AssignToInterviewAsync(this.input.InterviewId.Value, selectedResponsible);
                else if(this.input.AssignmentId.HasValue)
                    this.AssignToAssignment(this.input.AssignmentId.Value, selectedResponsible);
                else throw new NotSupportedException("Reassign dialog support interview or assignment only");
            }
            finally
            {
                this.Cancel();
            }
        }

        private void AssignToAssignment(int assignmentId, ResponsibleToSelectViewModel responsible)
        {
            var assignment = this.assignmentsStorage.GetById(assignmentId);

            assignment.ReceivedByInterviewerAt = null;
            assignment.ResponsibleId = responsible.Id;
            assignment.ResponsibleName = responsible.Login;
            assignment.Comments = this.Comments;

            this.assignmentsStorage.Store(assignment);

            this.messenger.Publish(new DashboardChangedMsg(this));
            this.auditLogService.Write(new AssignResponsibleToAssignmentAuditLogEntity(assignmentId, responsible.Id, responsible.Login));
        }

        private async Task AssignToInterviewAsync(Guid interviewId, ResponsibleToSelectViewModel responsible)
        {
            var interview = this.statefulInterviewRepository.Get(interviewId.FormatGuid());

            ICommand command = responsible.Role == UserRoles.Supervisor
                ? new AssignSupervisorCommand(interviewId, this.principal.CurrentUserIdentity.UserId, responsible.Id)
                : new AssignInterviewerCommand(interviewId, this.principal.CurrentUserIdentity.UserId, responsible.Id);

            this.commandService.Execute(command);

            this.auditLogService.Write(new AssignResponsibleToInterviewAuditLogEntity(interviewId,
                interview.GetInterviewKey().ToString(), responsible.Id, responsible.Login));

            await this.navigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }

        private void SelectResponsible(ResponsibleToSelectViewModel responsible)
        {
            this.UiItems.Where(x => x != responsible).ForEach(x => x.IsSelected = false);
            this.CanReassign = true;
        }

        private SelectResponsibleForAssignmentArgs input;
        public override void Prepare(SelectResponsibleForAssignmentArgs parameter)
        {
            this.input = parameter;

            var responsible = GetResponsible(parameter);
            var interviewersViewModels = this.usersRepository.LoadAll()
                .Where(x => x.InterviewerId != responsible 
                            && !x.IsLockedByHeadquarters 
                            && !x.IsLockedBySupervisor)
                .Select(ToInterviewerToSelectViewModel)
                .OrderBy(x => $"{x.FullName}{(string.IsNullOrEmpty(x.FullName) ? "" : " - ")}{x.Login}");

            var supervisorViewModel = GetSupervisorViewModel();
            var responsiblesViewModels = supervisorViewModel.ToEnumerable()
                .Concat(interviewersViewModels)
                .Where(x => x.Id != responsible)
                .ToList<ResponsibleToSelectViewModel>();
            this.UiItems = new MvxObservableCollection<ResponsibleToSelectViewModel>(responsiblesViewModels);
        }

        private ResponsibleToSelectViewModel GetSupervisorViewModel()
        {
            var userIdentity = principal.CurrentUserIdentity;
            
            var interviewsQuantityByInterviewer = this.assignmentsStorage.WhereSelect(
                @where => @where.ResponsibleId == userIdentity.UserId,
                @select =>
                    new InterviewsQuantity
                    {
                        Quantity = @select.Quantity,
                        CreatedInterviewsCount = @select.CreatedInterviewsCount
                    });

            var assignmentsCount = interviewsQuantityByInterviewer.Sum(x =>
                x.Quantity == null ? 1 : x.Quantity.GetValueOrDefault() - x.CreatedInterviewsCount.GetValueOrDefault());

            var interviewsCount = this.interviewStorage.Count(y => y.ResponsibleId == userIdentity.UserId);
            
            return new ResponsibleToSelectViewModel(this)
            {
                Id = userIdentity.UserId,
                Login = userIdentity.Name,
                Role = UserRoles.Supervisor,
                InterviewsCount = interviewsCount + assignmentsCount,
            };
        }

        private Guid? GetResponsible(SelectResponsibleForAssignmentArgs args)
        {
            if (args.InterviewId.HasValue)
            {
                var selectedInterview = this.statefulInterviewRepository.Get(this.input.InterviewId.Value.FormatGuid());
                return selectedInterview.CurrentResponsibleId;
            }

            if (args.AssignmentId.HasValue)
            {
                var selectedAssignment = this.assignmentsStorage.GetById(this.input.AssignmentId.Value);
                return selectedAssignment.ResponsibleId;
            }

            throw new NotSupportedException("Reassign dialog support interview or assignment only");
        }

        private class InterviewsQuantity
        {
            public int? Quantity { get; set; }
            public int? CreatedInterviewsCount { get; set; }
        }
        private ResponsibleToSelectViewModel ToInterviewerToSelectViewModel(InterviewerDocument interviewer)
        {
            var interviewsQuantityByInterviewer = this.assignmentsStorage.WhereSelect(
                @where => @where.ResponsibleId == interviewer.InterviewerId,
                @select =>
                new InterviewsQuantity
                {
                    Quantity = @select.Quantity,
                    CreatedInterviewsCount = @select.CreatedInterviewsCount
                });

            var assignmentsCount = interviewsQuantityByInterviewer.Sum(x =>
                x.Quantity == null ? 1 : x.Quantity.GetValueOrDefault() - x.CreatedInterviewsCount.GetValueOrDefault());

            var interviewsCount = this.interviewStorage.Count(y =>
                y.ResponsibleId == interviewer.InterviewerId && 
                (y.Status == InterviewStatus.RejectedBySupervisor || y.Status == InterviewStatus.RejectedByHeadquarters));

            return new ResponsibleToSelectViewModel(this)
            {
                Id = interviewer.InterviewerId,
                Login = interviewer.UserName,
                FullName = interviewer.FullaName,
                InterviewsCount = interviewsCount + assignmentsCount,
                Role = UserRoles.Interviewer,
            };
        }
    }
}
