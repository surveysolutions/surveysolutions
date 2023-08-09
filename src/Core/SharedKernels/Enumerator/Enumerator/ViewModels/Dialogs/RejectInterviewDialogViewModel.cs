using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class RejectInterviewDialogViewModel: ActionDialogViewModel<RejectInterviewDialogArgs>
{
    private readonly IStatefulInterviewRepository statefulInterviewRepository;
    private readonly ICommandService commandService;
    private readonly IAuditLogService auditLogService;
    private readonly IMvxMessenger messenger;

    public RejectInterviewDialogViewModel(IMvxNavigationService mvxMvxNavigationService,
        IStatefulInterviewRepository statefulInterviewRepository,
        ICommandService commandService,
        IPrincipal principal,
        IAuditLogService auditLogService,
        IMvxMessenger messenger,
        IPlainStorage<InterviewerDocument> usersRepository,
        IPlainStorage<InterviewView> interviewStorage, 
        IPlainStorage<AssignmentDocument, int> assignmentsStorage
        ) : base(mvxMvxNavigationService, principal, interviewStorage, assignmentsStorage, usersRepository)
    {
        this.statefulInterviewRepository = statefulInterviewRepository;
        this.commandService = commandService;
        this.auditLogService = auditLogService;
        this.messenger = messenger;
    }

    public override string DialogTitle => UIResources.Supervisor_Complete_Reject_btn;
    public override string ApplyTitle => UIResources.Supervisor_Complete_Reject_btn;

    public override bool ShowResponsibles => true;
    public override bool NeedAddSupervisorToResponsibles => false;
    public override bool CanApply => true;

    protected override async Task ApplyAsync()
    {
        try
        {
            var interviewId = this.CreateParameter.InterviewId;
            var interview = this.statefulInterviewRepository.GetOrThrow(interviewId.FormatGuid());

            var selectedResponsible = this.ResponsibleItems.SingleOrDefault(x => x.IsSelected);
            bool isNewResponsibleSelected = selectedResponsible != null && selectedResponsible.Id != interview.CurrentResponsibleId;

            var currentUserId = this.Principal.CurrentUserIdentity.UserId;
            ICommand command = isNewResponsibleSelected
                ? new RejectInterviewToInterviewerCommand(currentUserId, interviewId, selectedResponsible.Id, Comments)
                : new RejectInterviewCommand(interviewId, currentUserId, Comments);

            await this.commandService.ExecuteAsync(command);

            IAuditLogEntity logEntity = isNewResponsibleSelected
                ? new RejectInterviewToInterviewerAuditLogEntity(interviewId, interview.GetInterviewKey().ToString(), selectedResponsible.Id, selectedResponsible.Login)
                : new RejectInterviewAuditLogEntity(interviewId, interview.GetInterviewKey().ToString());
                
            this.auditLogService.Write(logEntity);

            this.messenger.Publish(new DashboardChangedMsg(this) { InterviewId = interviewId });
        }
        finally
        {
            await this.Cancel();
        }
    }
}
