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
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class AssignInterviewDialogViewModel: DoActionDialogViewModel<AssignInterviewDialogArgs>
{
    private readonly IStatefulInterviewRepository statefulInterviewRepository;
    private readonly ICommandService commandService;
    private readonly IPrincipal principal;
    private readonly IAuditLogService auditLogService;
    private readonly IMvxMessenger messenger;
    private readonly IPlainStorage<InterviewerDocument> usersRepository;

    public AssignInterviewDialogViewModel(IMvxNavigationService mvxMvxNavigationService,
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
        this.principal = principal;
        this.auditLogService = auditLogService;
        this.messenger = messenger;
        this.usersRepository = usersRepository;
    }

    public override string DialogTitle => EnumeratorUIResources.SelectResponsible_Reassign;
    public override string ApplyTitle => EnumeratorUIResources.SelectResponsible_ReassignButtonText;
    public override string CommentHelperText => EnumeratorUIResources.SelectResponsible_ReassignDescription;

    public override bool ShowResponsibles => true;

    public override void Prepare(AssignInterviewDialogArgs parameter)
    {
        base.Prepare(parameter);
        
        var interviewId = CreateParameter.InterviewId;
        var interviewView = InterviewStorage.GetById(interviewId.FormatGuid());

        var needShowConfirm = interviewView.ReceivedByInterviewerAtUtc.HasValue;
        if (needShowConfirm)
        {
            var responsible = usersRepository.GetById(interviewView.ResponsibleId.FormatGuid());
            var timeSpan = DateTime.UtcNow - interviewView.ReceivedByInterviewerAtUtc.Value;
            ConfirmText = string.Format(UIResources.Interviewer_Reassign_AlreadyReceivedAssignment,
                responsible.UserName,
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
            var interviewId = CreateParameter.InterviewId;
            var interview = this.statefulInterviewRepository.GetOrThrow(interviewId.FormatGuid());

            ICommand command = responsible.Role == UserRoles.Supervisor
                ? new AssignSupervisorCommand(interviewId, this.principal.CurrentUserIdentity.UserId, responsible.Id)
                : new AssignInterviewerCommand(interviewId, this.principal.CurrentUserIdentity.UserId, responsible.Id);

            this.commandService.Execute(command);

            this.auditLogService.Write(new AssignResponsibleToInterviewAuditLogEntity(interviewId,
                interview.GetInterviewKey().ToString(), responsible.Id, responsible.Login));

            this.messenger.Publish(new DashboardChangedMsg(this));
        }
        finally
        {
            this.Cancel();
        }
        
        return Task.CompletedTask;
    }

    protected override IEnumerable<InterviewerDocument> GetInterviewers(AssignInterviewDialogArgs parameter, out bool needAddSupervisorToResponsibles)
    {
        var interviewId = parameter.InterviewId;
        var interview = this.statefulInterviewRepository.GetOrThrow(interviewId.FormatGuid());
        var responsibleId = interview.CurrentResponsibleId;

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