using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Localisation;
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

            this.messenger.Publish(new DashboardChangedMsg(this) { AssignmentId = assignmentId });
            this.auditLogService.Write(new AssignResponsibleToAssignmentAuditLogEntity(assignmentId, responsible.Id, responsible.Login));
        }
        finally
        {
            this.Cancel();
        }
        
        return Task.CompletedTask;
    }

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