using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
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

public class AssignAssignmentDialogViewModel: DoActionDialogViewModel<AssignAssignmentDialogArgs>
{
    private readonly IPrincipal principal;
    private readonly IAuditLogService auditLogService;
    private readonly IMvxMessenger messenger;
    private readonly IPlainStorage<InterviewerDocument> usersRepository;

    public AssignAssignmentDialogViewModel(IMvxNavigationService mvxMvxNavigationService,
        IPrincipal principal,
        IAuditLogService auditLogService,
        IMvxMessenger messenger,
        IPlainStorage<InterviewerDocument> usersRepository,
        IPlainStorage<InterviewView> interviewStorage, 
        IPlainStorage<AssignmentDocument, int> assignmentsStorage
        ) : base(mvxMvxNavigationService, principal, interviewStorage, assignmentsStorage)
    {
        this.principal = principal;
        this.auditLogService = auditLogService;
        this.messenger = messenger;
        this.usersRepository = usersRepository;
    }

    public override string DialogTitle => EnumeratorUIResources.SelectResponsible_Reassign;
    public override string ApplyTitle => EnumeratorUIResources.SelectResponsible_ReassignButtonText;
    public override string CommentHint => UIResources.Interviewer_Reassign_Comment;
    public override string CommentHelperText => UIResources.Interviewer_Reassign_Comment;

    protected override Task DoApplyAsync()
    {
        if (!this.CanApply) 
            return Task.CompletedTask;
        this.CanApply = false;

        var responsible = this.ResponsibleItems.Single(x => x.IsSelected);

        try
        {
            var assignmentId = CreateParameter.AssignmentId;
            var assignment = this.AssignmentsStorage.GetById(assignmentId);

            assignment.ReceivedByInterviewerAt = null;
            assignment.ResponsibleId = responsible.Id;
            assignment.ResponsibleName = responsible.Login;
            assignment.Comments = this.Comments;

            this.AssignmentsStorage.Store(assignment);

            this.messenger.Publish(new DashboardChangedMsg(this));
            this.auditLogService.Write(new AssignResponsibleToAssignmentAuditLogEntity(assignmentId, responsible.Id, responsible.Login));
        }
        finally
        {
            this.Cancel();
        }
        
        return Task.CompletedTask;
    }

    public override bool ShowResponsibles => true;

    protected override IEnumerable<InterviewerDocument> GetInterviewers(AssignAssignmentDialogArgs parameter, out bool needAddSupervisorToResponsibles)
    {
        var assignmentId = parameter.AssignmentId;
        var selectedAssignment = this.AssignmentsStorage.GetById(assignmentId);
        var responsibleId = selectedAssignment.ResponsibleId;

        var interviewersViewModels = this.usersRepository.LoadAll()
            .Where(x => x.InterviewerId != responsibleId
                        && !x.IsLockedByHeadquarters
                        && !x.IsLockedBySupervisor);
            
        var userIdentity = principal.CurrentUserIdentity;
        needAddSupervisorToResponsibles = userIdentity.UserId != responsibleId;
        
        return interviewersViewModels;
    }

    protected override void ResponsibleSelected(ResponsibleToSelectViewModel responsible)
    {
        base.ResponsibleSelected(responsible);

        CanApply = responsible != null;
    }
}