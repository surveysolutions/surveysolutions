using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services
{
    [TestFixture]
    [TestOf(typeof(AudioAuditRecordingExecutor))]
    public class AudioAuditRecordingExecutorTests
    {
        private static AudioAuditRecordingExecutor CreateExecutor(IAudioAuditService audioAuditService = null)
            => new AudioAuditRecordingExecutor(audioAuditService ?? Substitute.For<IAudioAuditService>());

        [Test]
        public async Task when_target_is_whole_interview_should_start_recording_once()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var startCount = 0;
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            await executor.EvaluateAsync(interviewId,
                () => RecordingTarget.WholeInterview,
                _ => { startCount++; return Task.FromResult(true); },
                CancellationToken.None);

            Assert.That(startCount, Is.EqualTo(1));
            audioAuditService.DidNotReceiveWithAnyArgs().StopAudioRecording(default);
        }

        [Test]
        public async Task when_target_is_none_should_not_start_recording()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var startCount = 0;
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;

            await executor.EvaluateAsync(Guid.NewGuid(),
                () => RecordingTarget.None,
                _ => { startCount++; return Task.FromResult(true); },
                CancellationToken.None);

            Assert.That(startCount, Is.EqualTo(0));
            audioAuditService.DidNotReceiveWithAnyArgs().StopAudioRecording(default);
        }

        [Test]
        public async Task when_view_is_not_visible_should_not_start_recording_even_if_target_requests_it()
        {
            var startCount = 0;
            var executor = CreateExecutor();
            executor.IsViewVisible = false;

            await executor.EvaluateAsync(Guid.NewGuid(),
                () => RecordingTarget.WholeInterview,
                _ => { startCount++; return Task.FromResult(true); },
                CancellationToken.None);

            Assert.That(startCount, Is.EqualTo(0));
        }

        [Test]
        public async Task when_evaluated_with_same_target_again_should_not_restart_recording()
        {
            var startCount = 0;
            var groupId = Guid.NewGuid();
            var executor = CreateExecutor();
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            Task<bool> Start(Guid _) { startCount++; return Task.FromResult(true); }

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.Group(groupId), Start, CancellationToken.None);
            await executor.EvaluateAsync(interviewId, () => RecordingTarget.Group(groupId), Start, CancellationToken.None);

            Assert.That(startCount, Is.EqualTo(1));
        }

        [Test]
        public async Task when_target_group_changes_should_stop_previous_and_start_new_one()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var startedGroups = new List<RecordingTarget>();
            var firstGroup = Guid.NewGuid();
            var secondGroup = Guid.NewGuid();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            var target = RecordingTarget.Group(firstGroup);
            Task<bool> Start(Guid _) { startedGroups.Add(target); return Task.FromResult(true); }

            await executor.EvaluateAsync(interviewId, () => target, Start, CancellationToken.None);

            target = RecordingTarget.Group(secondGroup);
            await executor.EvaluateAsync(interviewId, () => target, Start, CancellationToken.None);

            audioAuditService.Received(1).StopAudioRecording(interviewId);
            Assert.That(startedGroups.Count, Is.EqualTo(2));
            Assert.That(startedGroups[0].GroupId, Is.EqualTo(firstGroup));
            Assert.That(startedGroups[1].GroupId, Is.EqualTo(secondGroup));
        }

        [Test]
        public async Task when_leaving_scope_to_none_should_stop_active_recording()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var groupId = Guid.NewGuid();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.Group(groupId),
                _ => Task.FromResult(true), CancellationToken.None);

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.None,
                _ => Task.FromResult(true), CancellationToken.None);

            audioAuditService.Received(1).StopAudioRecording(interviewId);
        }

        [Test]
        public async Task when_start_fails_should_leave_target_none_so_next_evaluation_retries()
        {
            var startCount = 0;
            var executor = CreateExecutor();
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            // First start fails, second start succeeds.
            Task<bool> Start(Guid _)
            {
                startCount++;
                return Task.FromResult(startCount > 1);
            }

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview, Start, CancellationToken.None);
            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview, Start, CancellationToken.None);

            Assert.That(startCount, Is.EqualTo(2), "a failed start must not be treated as an active recording");
        }

        [Test]
        public async Task when_view_disappears_while_starting_should_stop_the_just_started_recording()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            // Simulate the view disappearing during the start dispatch.
            Task<bool> Start(Guid _)
            {
                executor.IsViewVisible = false;
                return Task.FromResult(true);
            }

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview, Start, CancellationToken.None);

            audioAuditService.Received(1).StopAudioRecording(interviewId);
        }

        [Test]
        public async Task when_stop_requested_after_view_disappeared_should_stop_active_recording()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview,
                _ => Task.FromResult(true), CancellationToken.None);

            executor.IsViewVisible = false;
            await executor.StopAsync(interviewId);

            audioAuditService.Received(1).StopAudioRecording(interviewId);
        }

        [Test]
        public async Task when_stop_requested_but_view_became_visible_again_should_not_stop_recording()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview,
                _ => Task.FromResult(true), CancellationToken.None);

            // User navigated back quickly: view is visible again when the stale stop runs.
            await executor.StopAsync(interviewId);

            audioAuditService.DidNotReceiveWithAnyArgs().StopAudioRecording(default);
        }

        [Test]
        public void cancellation_token_is_signalled_after_cancel()
        {
            var executor = CreateExecutor();

            Assert.That(executor.CancellationToken.IsCancellationRequested, Is.False);
            executor.Cancel();
            Assert.That(executor.CancellationToken.IsCancellationRequested, Is.True);
        }

        [Test]
        public void when_cancel_called_after_dispose_should_not_throw()
        {
            var executor = CreateExecutor();

            executor.Dispose();

            Assert.DoesNotThrow(() => executor.Cancel());
        }

        [Test]
        public void when_disposed_more_than_once_should_not_throw()
        {
            var executor = CreateExecutor();

            executor.Dispose();

            Assert.DoesNotThrow(() => executor.Dispose());
        }
    }
}
