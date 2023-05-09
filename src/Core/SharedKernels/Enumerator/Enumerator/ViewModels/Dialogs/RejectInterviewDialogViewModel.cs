using System.Collections.Generic;
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

public class RejectInterviewDialogViewModel: DoActionDialogViewModel<RejectInterviewDialogArgs>
{
    private readonly IStatefulInterviewRepository statefulInterviewRepository;
    private readonly ICommandService commandService;
    private readonly IAuditLogService auditLogService;
    private readonly IMvxMessenger messenger;
    private readonly IPlainStorage<InterviewerDocument> usersRepository;

    public RejectInterviewDialogViewModel(IMvxNavigationService mvxMvxNavigationService,
        IStatefulInterviewRepository statefulInterviewRepository,
        ICommandService commandService,
        IPrincipal principal,
        IAuditLogService auditLogService,
        IMvxMessenger messenger,
        IPlainStorage<InterviewerDocument> usersRepository,
        IPlainStorage<InterviewView> interviewStorage, 
        IPlainStorage<AssignmentDocument, int> assignmentsStorage
        ) : base(mvxMvxNavigationService, principal, interviewStorage, assignmentsStorage)
    {
        this.statefulInterviewRepository = statefulInterviewRepository;
        this.commandService = commandService;
        this.auditLogService = auditLogService;
        this.messenger = messenger;
        this.usersRepository = usersRepository;
    }

    public override string DialogTitle => UIResources.Supervisor_Complete_Reject_btn;

    public override string ApplyTitle => UIResources.Supervisor_Complete_Reject_btn;

    public override bool ShowResponsibles => true;

    public override bool CanApply => true;

    protected override Task DoApplyAsync()
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

            this.commandService.Execute(command);

            IAuditLogEntity logEntity = isNewResponsibleSelected
                ? new RejectInterviewToInterviewerAuditLogEntity(interviewId, interview.GetInterviewKey().ToString(), selectedResponsible.Id, selectedResponsible.Login)
                : new RejectInterviewAuditLogEntity(interviewId, interview.GetInterviewKey().ToString());
                
            this.auditLogService.Write(logEntity);

            this.messenger.Publish(new DashboardChangedMsg(this));
        }
        finally
        {
            this.Cancel();
        }
        
        return Task.CompletedTask;
    }

    protected override IEnumerable<InterviewerDocument> GetInterviewers(RejectInterviewDialogArgs parameter, out bool needAddSupervisorToResponsibles)
    {
        needAddSupervisorToResponsibles = false;
        
        var interviewId = parameter.InterviewId;
        var interview = this.statefulInterviewRepository.GetOrThrow(interviewId.FormatGuid());

        var interviewersViewModels = this.usersRepository.LoadAll()
            .Where(x => x.InterviewerId != interview.CurrentResponsibleId 
                        && !x.IsLockedByHeadquarters 
                        && !x.IsLockedBySupervisor);

        return interviewersViewModels;
    }
}