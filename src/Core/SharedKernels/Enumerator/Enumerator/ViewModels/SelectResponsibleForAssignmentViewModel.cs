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
        private readonly IStatefulInterviewRepository interviewRepository;
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
            IStatefulInterviewRepository interviewRepository,
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
            this.interviewRepository = interviewRepository;
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

        private MvxObservableCollection<InterviewerToSelectViewModel> uiItems = new MvxObservableCollection<InterviewerToSelectViewModel>();
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

            var selectedInterviewer = this.uiItems.First(x => x.IsSelected);

            try
            {
                if (this.input.InterviewId.HasValue)
                    await this.AssignToInterviewAsync(this.input.InterviewId.Value, selectedInterviewer);
                else if(this.input.AssignmentId.HasValue)
                    this.AssignToAssignment(this.input.AssignmentId.Value, selectedInterviewer);
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

            this.auditLogService.Write(new AssignResponsibleToAssignmentAuditLogEntity(assignmentId, interviewer.Id, interviewer.Login));
            this.mvxMessenger.Publish(new DashboardChangedMsg(this));
        }

        private async Task AssignToInterviewAsync(Guid interviewId, InterviewerToSelectViewModel interviewer)
        {
            var interview = this.interviewRepository.Get(interviewId.FormatGuid());

            var command = new AssignInterviewerCommand(interviewId, this.principal.CurrentUserIdentity.UserId,
                interviewer.Id);

            this.auditLogService.Write(new AssignResponsibleToInterviewAuditLogEntity(interviewId,
                interview.GetInterviewKey().ToString(), interviewer.Id, interviewer.Login));

            this.commandService.Execute(command);

            await this.navigationService.NavigateToDashboardAsync(interviewId.FormatGuid());
        }

        private void SelectInterviewer(InterviewerToSelectViewModel interviewer)
        {
            this.uiItems.Where(x => x != interviewer).ForEach(x => x.IsSelected = false);
            this.CanReassign = true;
        }

        private SelectResponsibleForAssignmentArgs input;
        public override void Prepare(SelectResponsibleForAssignmentArgs parameter)
        {
            this.input = parameter;

            var allInterviewers = usersRepository.LoadAll().AsQueryable();

            if (this.input.InterviewId.HasValue)
            {
                var selectedInterview = this.interviewRepository.Get(this.input.InterviewId.Value.FormatGuid());
                allInterviewers = allInterviewers.Where(x => x.InterviewerId != selectedInterview.CurrentResponsibleId);
            }
            
            if(this.input.AssignmentId.HasValue)
            {
                var selectedAssigment = this.assignmentsStorage.GetById(this.input.AssignmentId.Value);
                allInterviewers = allInterviewers.Where(x => x.InterviewerId != selectedAssigment.ResponsibleId);
            }

            
            this.UiItems.ReplaceWith(allInterviewers.AsEnumerable().Select(ToInterviewerToSelectViewModel));
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
                @select => new InterviewsQuantity
                {
                    Quantity = @select.Quantity,
                    CreatedInterviewsCount = @select.CreatedInterviewsCount
                });

            var assignmentsCount = interviewsQuantityByInterviewer.Sum(x =>
                x.Quantity == null ? 1 : x.Quantity.GetValueOrDefault() - x.CreatedInterviewsCount.GetValueOrDefault());

            var interviewsCount = this.interviewStorage.Count(y => y.ResponsibleId == interviewer.InterviewerId);

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
