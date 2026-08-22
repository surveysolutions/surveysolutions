using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
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
    public void when_user_is_observer_should_block_access()
    {
        var interviewId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId);

        var subject = CreateSubject(
            interviewId,
            interview,
            Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsObserver));

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

    [Test]
    public async Task when_segments_have_no_timestamp_should_order_by_filename_ordinally()
    {
        var interviewId = Guid.NewGuid();
        var interview = Create.AggregateRoot.StatefulInterview(interviewId: interviewId);
        var audioStorage = new Mock<IAudioAuditFileStorage>();
        var descriptors = new List<InterviewBinaryDataDescriptor>
        {
            CreateDescriptor(interviewId, "ä-segment.m4a"),
            CreateDescriptor(interviewId, "z-segment.m4a"),
        };
        audioStorage.Setup(x => x.GetBinaryFilesForInterview(interviewId)).ReturnsAsync(descriptors);

        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        CultureInfo.CurrentUICulture = new CultureInfo("de-DE");
        try
        {
            var subject = CreateSubject(
                interviewId,
                interview,
                Mock.Of<IAuthorizedUser>(user => user.IsAuthenticated && user.IsAdministrator),
                audioAuditFileStorage: audioStorage.Object);

            var orderedSegments = await subject.GetAudioAuditSegmentsAsync(interviewId);

            var orderedFileNames = orderedSegments
                .Select(segment => DecodeOpaqueId(segment.SegmentId))
                .ToArray();

            Assert.That(orderedFileNames, Is.EqualTo(new[] { "z-segment.m4a", "ä-segment.m4a" }));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    private static AudioAuditAccessService CreateSubject(
        Guid interviewId,
        StatefulInterview interview,
        IAuthorizedUser authorizedUser,
        InterviewerSettings settings = null,
        IAudioAuditFileStorage audioAuditFileStorage = null)
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
            audioAuditFileStorage ?? Mock.Of<IAudioAuditFileStorage>(),
            settingsStorage,
            reviewAllowedService.Object);
    }

    private static InterviewBinaryDataDescriptor CreateDescriptor(Guid interviewId, string fileName)
        => new InterviewBinaryDataDescriptor(
            interviewId,
            fileName,
            "audio/mp4",
            () => Task.FromResult(Array.Empty<byte>()),
            null,
            null);

    private static string DecodeOpaqueId(string opaqueId)
    {
        var padded = opaqueId.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }
}
