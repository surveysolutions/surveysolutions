using System;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class AssignAssignmentDialogViewModel: ActionDialogViewModel<AssignAssignmentDialogArgs>
{
    private readonly IAuditLogService auditLogService;
    private readonly IMvxMessenger messenger;

    public AssignAssignmentDialogViewModel(IMvxNavigationService mvxMvxNavigationService,
        IPrincipal principal,
        IAuditLogService auditLogService,
        IMvxMessenger messenger,
        IPlainStorage<InterviewerDocument> usersRepository,
        IPlainStorage<InterviewView> interviewStorage, 
        IPlainStorage<AssignmentDocument, int> assignmentsStorage
        ) : base(mvxMvxNavigationService, principal, interviewStorage, assignmentsStorage, usersRepository)
    {
        this.auditLogService = auditLogService;
        this.messenger = messenger;
    }

    public override string DialogTitle => UIResources.Supervisor_Complete_Assign_btn;
    public override string ApplyTitle => UIResources.Supervisor_Complete_Assign_btn;
    public override string ResponsiblesTitle => EnumeratorUIResources.SelectResponsible_ReassignDescription;

    public override bool ShowResponsibles => true;

    public override void Prepare(AssignAssignmentDialogArgs parameter)
    {
        base.Prepare(parameter);
        
        var assignmentId = CreateParameter.AssignmentId;
        var assignmentDocument = AssignmentsStorage.GetById(assignmentId);
        var needShowConfirm = assignmentDocument.ReceivedByInterviewerAt.HasValue;
        if (needShowConfirm)
        {
            var timeSpan = DateTime.UtcNow - assignmentDocument.ReceivedByInterviewerAt.Value;
            ConfirmText = string.Format(UIResources.Interviewer_Reassign_AlreadyReceivedAssignment,
                assignmentDocument.ResponsibleName,
                timeSpan.Humanize(minUnit: TimeUnit.Second));
            ShowConfirm = true;
        }
    }

    protected override Task ApplyAsync()
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

            this.messenger.Publish(new DashboardChangedMsg(this) { AssignmentId = assignmentId });
            this.auditLogService.Write(new AssignResponsibleToAssignmentAuditLogEntity(assignmentId, responsible.Id, responsible.Login));
        }
        finally
        {
            this.Cancel();
        }
        
        return Task.CompletedTask;
    }

    protected override Guid? GetCurrentEntityResponsible(AssignAssignmentDialogArgs parameter)
    {
        var assignmentId = parameter.AssignmentId;
        var selectedAssignment = this.AssignmentsStorage.GetById(assignmentId);
        return selectedAssignment.ResponsibleId;
    }

    protected override void ResponsibleSelected(ResponsibleToSelectViewModel responsible)
    {
        base.ResponsibleSelected(responsible);

        CanApply = ShowConfirm 
            ? responsible != null && IsConfirmed
            : responsible != null;
    }

    protected override void ConfirmStateChanged(bool newValue)
    {
        base.ConfirmStateChanged(newValue);

        var responsible = ResponsibleItems.SingleOrDefault(o => o.IsSelected); 
        CanApply = ShowConfirm 
            ? responsible != null && newValue
            : responsible != null;
    }
}