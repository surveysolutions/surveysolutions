using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class ApproveInterviewDialogViewModel : ActionDialogViewModel<ApproveInterviewDialogArgs>
{
    private readonly IStatefulInterviewRepository statefulInterviewRepository;
    private readonly ICommandService commandService;
    private readonly IAuditLogService auditLogService;
    private readonly IMvxMessenger messenger;

    public ApproveInterviewDialogViewModel(IMvxNavigationService mvxMvxNavigationService,
        IStatefulInterviewRepository statefulInterviewRepository,
        ICommandService commandService,
        IPrincipal principal,
        IAuditLogService auditLogService,
        IMvxMessenger messenger,
        IPlainStorage<InterviewView> interviewStorage, 
        IPlainStorage<AssignmentDocument, int> assignmentsStorage,
        IPlainStorage<InterviewerDocument> usersRepository
        ) : base(mvxMvxNavigationService, principal, interviewStorage, assignmentsStorage, usersRepository)
    {
        this.statefulInterviewRepository = statefulInterviewRepository;
        this.commandService = commandService;
        this.auditLogService = auditLogService;
        this.messenger = messenger;
    }

    public override string DialogTitle => UIResources.Supervisor_Complete_Approve_btn;
    public override string ApplyTitle => UIResources.Supervisor_Complete_Approve_btn;

    public override bool ShowResponsibles => false;

    public override bool CanApply => true;

    protected override Task ApplyAsync()
    {
        try
        {
            var interviewId = this.CreateParameter.InterviewId;
            var interview = this.statefulInterviewRepository.GetOrThrow(interviewId.FormatGuid());

            var command = new ApproveInterviewCommand(interviewId, this.Principal.CurrentUserIdentity.UserId, Comments);

            this.commandService.Execute(command);

            this.auditLogService.Write(new ApproveInterviewAuditLogEntity(interviewId,
                interview.GetInterviewKey().ToString()));

            this.messenger.Publish(new DashboardChangedMsg(this) { InterviewId = interviewId });
        }
        finally
        {
            this.Cancel();
        }
        
        return Task.CompletedTask;
    }
}