﻿using System;
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
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;

public class AssignInterviewDialogViewModel: ActionDialogViewModel<AssignInterviewDialogArgs>
{
    private readonly IStatefulInterviewRepository statefulInterviewRepository;
    private readonly ICommandService commandService;
    private readonly IPrincipal principal;
    private readonly IAuditLogService auditLogService;
    private readonly IMvxMessenger messenger;

    public AssignInterviewDialogViewModel(IMvxNavigationService mvxMvxNavigationService,
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
        this.principal = principal;
        this.auditLogService = auditLogService;
        this.messenger = messenger;
    }

    public override string DialogTitle => UIResources.Supervisor_Complete_Assign_btn;
    public override string ApplyTitle => UIResources.Supervisor_Complete_Assign_btn;
    public override string ResponsiblesTitle => EnumeratorUIResources.SelectResponsible_ReassignDescription;

    public override bool ShowResponsibles => true;

    public override void Prepare(AssignInterviewDialogArgs parameter)
    {
        base.Prepare(parameter);
        
        var interviewId = CreateParameter.InterviewId;
        var interviewView = InterviewStorage.GetById(interviewId.FormatGuid());

        var needShowConfirm = interviewView.ReceivedByInterviewerAtUtc.HasValue;
        if (needShowConfirm)
        {
            var responsible = UsersRepository.GetById(interviewView.ResponsibleId.FormatGuid());
            var timeSpan = DateTime.UtcNow - interviewView.ReceivedByInterviewerAtUtc.Value;
            ConfirmText = string.Format(UIResources.Interviewer_Reassign_AlreadyReceivedAssignment,
                responsible.UserName,
                NumericTextFormatter.FormatTimeHumanized(timeSpan));
            ShowConfirm = true;
        }
    }

    protected override async Task ApplyAsync()
    {
        if (!this.CanApply) 
            return;
        this.CanApply = false;

        var responsible = this.ResponsibleItems.Single(x => x.IsSelected);

        try
        {
            var interviewId = CreateParameter.InterviewId;
            var interview = this.statefulInterviewRepository.GetOrThrow(interviewId.FormatGuid());

            ICommand command = responsible.Role == UserRoles.Supervisor
                ? new AssignSupervisorCommand(interviewId, this.principal.CurrentUserIdentity.UserId, responsible.Id)
                : new AssignInterviewerCommand(interviewId, this.principal.CurrentUserIdentity.UserId, responsible.Id);

            await this.commandService.ExecuteAsync(command);

            this.auditLogService.Write(new AssignResponsibleToInterviewAuditLogEntity(interviewId,
                interview.GetInterviewKey().ToString(), responsible.Id, responsible.Login));

            this.messenger.Publish(new DashboardChangedMessage(this) { InterviewId = interviewId });
        }
        finally
        {
            await this.Cancel();
        }
    }

    protected override Guid? GetCurrentEntityResponsible(AssignInterviewDialogArgs parameter)
    {
        var interviewId = parameter.InterviewId;
        var interview = this.statefulInterviewRepository.GetOrThrow(interviewId.FormatGuid());
        return interview.CurrentResponsibleId;
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
