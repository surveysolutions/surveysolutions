using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.UI.Headquarters.Services;
using WB.UI.Headquarters.Services.Impl;
using WB.Enumerator.Native.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters;

[TestOf(typeof(AudioAuditAccessService))]
public class AudioAuditAccessServiceTests
{
    [Test]
    public void when_user_is_admin_should_allow_access()
    {
        var interviewId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId);

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsAdministrator));

        Assert.That(subject.CanAccessAudioAudit(interviewId), Is.True);
    }

    [Test]
    public void when_user_is_headquarter_should_allow_access()
    {
        var interviewId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId);

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsHeadquarter));

        Assert.That(subject.CanAccessAudioAudit(interviewId), Is.True);
    }

    [Test]
    public void when_supervisor_setting_is_missing_should_block_access()
    {
        var interviewId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId, supervisorId: supervisorId);

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsSupervisor && user.Id == supervisorId));

        Assert.That(subject.CanAccessAudioAudit(interviewId), Is.False);
    }

    [Test]
    public void when_supervisor_is_current_and_setting_enabled_should_allow_access()
    {
        var interviewId = Guid.NewGuid();
        var supervisorId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId, supervisorId: supervisorId);
        var settings = new InterviewerSettings { AllowSupervisorAudioAuditPlayback = true };

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsSupervisor && user.Id == supervisorId),
            settings);

        Assert.That(subject.CanAccessAudioAudit(interviewId), Is.True);
    }

    [Test]
    public void when_supervisor_is_not_current_should_block_access()
    {
        var interviewId = Guid.NewGuid();
        var currentSupervisorId = Guid.NewGuid();
        var formerSupervisorId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId, supervisorId: currentSupervisorId);
        var settings = new InterviewerSettings { AllowSupervisorAudioAuditPlayback = true };

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsSupervisor && user.Id == formerSupervisorId),
            settings);

        Assert.That(subject.CanAccessAudioAudit(interviewId), Is.False);
    }

    [Test]
    public void when_user_is_interviewer_should_block_access()
    {
        var interviewId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId);

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsInterviewer));

        Assert.That(subject.CanAccessAudioAudit(interviewId), Is.False);
    }

    [Test]
    public void when_user_is_anonymous_should_block_access()
    {
        var interviewId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId);

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated == false));

        Assert.That(subject.CanAccessAudioAudit(interviewId), Is.False);
    }

    private static AudioAuditAccessService CreateSubject(
        Guid interviewId,
        StatefulInterview interview,
        IAuthorizedUser authorizedUser,
        InterviewerSettings settings = null)
    {
        var settingsStorage = new InMemoryPlainStorageAccessor<InterviewerSettings>();
        if (settings != null)
            settingsStorage.Store(settings, WB.Core.BoundedContexts.Headquarters.Views.AppSetting.InterviewerSettings);

        var reviewAllowedService = new Mock<IReviewAllowedService>();
        reviewAllowedService
            .Setup(x => x.CheckIfAllowed(interviewId))
            .Callback(() =>
            {
                if (interview == null)
                    throw new InterviewAccessException(InterviewAccessExceptionReason.InterviewNotFound, string.Empty);

                if (authorizedUser.IsAdministrator || authorizedUser.IsHeadquarter)
                    return;

                if (authorizedUser.IsSupervisor && authorizedUser.Id == interview.SupervisorId)
                    return;

                throw new InterviewAccessException(InterviewAccessExceptionReason.UserNotAuthorised, string.Empty);
            });

        return new AudioAuditAccessService(
            authorizedUser,
            Mock.Of<IAudioAuditFileStorage>(),
            settingsStorage,
            reviewAllowedService.Object);
    }
}
