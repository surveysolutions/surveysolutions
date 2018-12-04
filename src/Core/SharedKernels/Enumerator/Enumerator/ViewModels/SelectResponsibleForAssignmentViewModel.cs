using System;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IMvxMessenger mvxMessenger;
        private readonly IStatefulInterviewRepository statefullInterviewRepository;
        private readonly IPlainStorage<InterviewView> interviewStorage;
        private readonly IPlainStorage<AssignmentDocument, int> assignmentsStorage;

        public SelectResponsibleForAssignmentViewModel(
            IMvxNavigationService mvxNavigationService,
            IPlainStorage<InterviewerDocument> usersRepository,
            IPrincipal principal,
            IAuditLogService auditLogService,
            ICommandService commandService,
            IViewModelNavigationService navigationService,
            IMvxMessenger mvxMessenger,
            IStatefulInterviewRepository statefullInterviewRepository,
            IPlainStorage<InterviewView> interviewStorage,
            IPlainStorage<AssignmentDocument, int> assignmentsStorage)
        {
            this.mvxNavigationService = mvxNavigationService;
            this.usersRepository = usersRepository;
            this.principal = principal;
            this.auditLogService = auditLogService;
            this.commandService = commandService;
            this.navigationService = navigationService;
            this.mvxMessenger = mvxMessenger;
            this.statefullInterviewRepository = statefullInterviewRepository;
            this.interviewStorage = interviewStorage;
            this.assignmentsStorage = assignmentsStorage;
        }

        private bool canReassign;
        public bool CanReassign
        {
            get => this.canReassign;
            protected set => this.RaiseAndSetIfChanged(ref this.canReassign, value);
        }
        private IMvxAsyncCommand reassignCommand;
        public IMvxAsyncCommand ReassignCommand => reassignCommand ??
                                                   (reassignCommand = new MvxAsyncCommand(this.ReassignAsync, () => this.CanReassign));
        public IMvxCommand CancelCommand => new MvxCommand(this.Cancel);
        public IMvxCommand SelectInterviewerCommand => new MvxCommand<InterviewerToSelectViewModel>(this.SelectInterviewer);

        private MvxObservableCollection<InterviewerToSelectViewModel> uiItems;
        public MvxObservableCollection<InterviewerToSelectViewModel> UiItems
        {
            get => this.uiItems;
            protected set => this.RaiseAndSetIfChanged(ref this.uiItems, value);
        }

        private void Cancel() => this.mvxNavigationService.Close(this);

        private async Task ReassignAsync()
        {
            if (!this.CanReassign) return;
            this.CanReassign = false;

            var selectedInterviewer = this.uiItems.Single(x => x.IsSelected);

            try
            {
                if (this.input.InterviewId.HasValue)
                    await this.AssignToInterviewAsync(this.input.InterviewId.Value, selectedInterviewer);
                else if(this.input.AssignmentId.HasValue)
                    this.AssignToAssignment(this.input.AssignmentId.Value, selectedInterviewer);
                else throw new NotSupportedException("Reassign dialog support interview or assignment only");
            }
            finally
            {
                this.Cancel();
            }
        }

        private void AssignToAssignment(int assignmentId, InterviewerToSelectViewModel interviewer)
        {
            var assignment = this.assignmentsStorage.GetById(assignmentId);

            assignment.ReceivedByInterviewerAt = null;
            assignment.ResponsibleId = interviewer.Id;
            assignment.ResponsibleName = interviewer.Login;

            this.assignmentsStorage.Store(assignment);

            this.mvxMessenger.Publish(new DashboardChangedMsg(this));
            this.auditLogService.Write(new AssignResponsibleToAssignmentAuditLogEntity(assignmentId, interviewer.Id, interviewer.Login));
        }

        private async Task AssignToInterviewAsync(Guid interviewId, InterviewerToSelectViewModel interviewer)
        {
            var interview = this.statefullInterviewRepository.Get(interviewId.FormatGuid());

            var command = new AssignInterviewerCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                interviewer.Id);

            this.commandService.Execute(command);

            this.auditLogService.Write(new AssignResponsibleToInterviewAuditLogEntity(interviewId,
                interview.GetInterviewKey().ToString(), interviewer.Id, interviewer.Login));

            await this.navigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }

        private void SelectInterviewer(InterviewerToSelectViewModel interviewer)
        {
            this.UiItems.Where(x => x != interviewer).ForEach(x => x.IsSelected = false);
            this.CanReassign = true;
        }

        private SelectResponsibleForAssignmentArgs input;
        public override void Prepare(SelectResponsibleForAssignmentArgs parameter)
        {
            this.input = parameter;

            var responsible = GetResponsible(parameter);
            var interviewerViewModels = this.usersRepository.LoadAll().Where(x => x.InterviewerId != responsible)
                .Select(ToInterviewerToSelectViewModel)
                .OrderBy(x => $"{x.FullName}{(string.IsNullOrEmpty(x.FullName) ? "" : " - ")}{x.Login}");

            this.UiItems = new MvxObservableCollection<InterviewerToSelectViewModel>(interviewerViewModels);
        }

        private Guid? GetResponsible(SelectResponsibleForAssignmentArgs args)
        {
            if (args.InterviewId.HasValue)
            {
                var selectedInterview = this.statefullInterviewRepository.Get(this.input.InterviewId.Value.FormatGuid());
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
        private InterviewerToSelectViewModel ToInterviewerToSelectViewModel(InterviewerDocument interviewer)
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

            return new InterviewerToSelectViewModel(this)
            {
                Id = interviewer.InterviewerId,
                Login = interviewer.UserName,
                FullName = interviewer.FullaName,
                InterviewsCount = interviewsCount + assignmentsCount
            };
        }
    }
}
