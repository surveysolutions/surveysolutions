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
            => new AudioAuditRecordingExecutor(audioAuditService ?? Substitute.For<IAudioAuditService>(),
                Substitute.For<WB.Core.GenericSubdomains.Portable.Services.ILogger>());

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
        public async Task when_start_throws_should_clear_guard_and_allow_subsequent_retry()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            var startCount = 0;

            // First evaluation: the start throws. The guard must be cleared so a later evaluation can retry.
            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview,
                _ =>
                {
                    startCount++;
                    throw new InvalidOperationException("recorder failed");
                },
                CancellationToken.None);

            Assert.That(startCount, Is.EqualTo(1));

            // Second evaluation: the start succeeds, proving the guard was released after the failure.
            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview,
                _ =>
                {
                    startCount++;
                    return Task.FromResult(true);
                },
                CancellationToken.None);

            Assert.That(startCount, Is.EqualTo(2));
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

        [Test]
        public async Task when_disposed_while_start_in_flight_should_stop_started_recording_and_not_throw()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            // The start completes successfully, but the executor is disposed while it is in flight (between
            // reserving the start and committing the target). The just-started recording must be stopped and
            // no ObjectDisposedException must leak from the disposed synchronization primitives.
            Task<bool> Start(Guid _)
            {
                executor.Dispose();
                return Task.FromResult(true);
            }

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview, Start, CancellationToken.None);

            audioAuditService.Received(1).StopAudioRecording(interviewId);

            // A subsequent evaluation on the disposed executor must return immediately (no operation is
            // started) rather than deadlocking on the now-disposed recording lock.
            var secondStartCount = 0;
            var secondEvaluation = executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview,
                _ => { secondStartCount++; return Task.FromResult(true); }, CancellationToken.None);

            Assert.That(secondEvaluation.IsCompleted, Is.True, "evaluation after disposal must not block");
            await secondEvaluation;
            Assert.That(secondStartCount, Is.EqualTo(0));
        }

        [Test]
        public async Task when_stop_starts_before_disposal_should_complete_stop_and_dispose_resources()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview,
                _ => Task.FromResult(true), CancellationToken.None);

            // Screen teardown: view hidden, stop issued, then Cancel/Dispose (as the interview view model does
            // on NavigateBack). The stop must still run to completion even though disposal follows immediately.
            executor.IsViewVisible = false;
            var stopTask = executor.StopAsync(interviewId);
            executor.Cancel();
            executor.Dispose();
            await stopTask;

            audioAuditService.Received(1).StopAudioRecording(interviewId);
            // The executor is now fully disposed; a second dispose must remain safe.
            Assert.DoesNotThrow(() => executor.Dispose());
        }

        [Test]
        public async Task when_cancelled_while_waiting_lock_should_not_start_recording()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();
            var startCount = 0;

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview,
                _ => { startCount++; return Task.FromResult(true); }, cts.Token);

            Assert.That(startCount, Is.EqualTo(0), "a cancelled evaluation must not start a recording");
            audioAuditService.DidNotReceiveWithAnyArgs().StopAudioRecording(default);
        }

        [Test]
        public async Task when_cancelled_after_start_should_stop_just_started_recording()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            using var cts = new CancellationTokenSource();

            // Cancellation is requested while the start is dispatching (mid permission dialog): the just-started
            // recording must be stopped rather than kept active.
            Task<bool> Start(Guid _)
            {
                cts.Cancel();
                return Task.FromResult(true);
            }

            await executor.EvaluateAsync(interviewId, () => RecordingTarget.WholeInterview, Start, cts.Token);

            audioAuditService.Received(1).StopAudioRecording(interviewId);
        }

        [Test]
        public async Task when_desired_target_toggles_faster_than_start_should_stop_after_iteration_cap()
        {
            var audioAuditService = Substitute.For<IAudioAuditService>();
            var executor = CreateExecutor(audioAuditService);
            executor.IsViewVisible = true;
            var interviewId = Guid.NewGuid();

            var startCount = 0;
            var groupA = Guid.NewGuid();
            var groupB = Guid.NewGuid();

            // A runaway provider that flips the desired group on every read. Without a cap this would loop
            // forever doing start/stop; the executor must bound the number of convergence iterations.
            RecordingTarget FlipFlopTarget()
                => RecordingTarget.Group(startCount % 2 == 0 ? groupA : groupB);

            await executor.EvaluateAsync(interviewId, FlipFlopTarget,
                _ => { startCount++; return Task.FromResult(true); }, CancellationToken.None);

            Assert.That(startCount, Is.LessThanOrEqualTo(10),
                "the convergence loop must be bounded to avoid thrashing the recorder");
        }
    }
}
