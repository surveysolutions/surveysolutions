using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class AudioAuditRecordingExecutor : IAudioAuditRecordingExecutor
    {
        private readonly IAudioAuditService audioAuditService;

        private bool isAuditStarting;
        private RecordingTarget currentRecordingTarget = RecordingTarget.None;
        private readonly SemaphoreSlim audioRecordingLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        public AudioAuditRecordingExecutor(IAudioAuditService audioAuditService)
        {
            this.audioAuditService = audioAuditService ?? throw new ArgumentNullException(nameof(audioAuditService));
        }

        public Task StartAudioRecordingAsync(Guid interviewId)
            => this.audioAuditService.StartAudioRecordingAsync(interviewId);

        public void StopAudioRecording(Guid interviewId)
            => this.audioAuditService.StopAudioRecording(interviewId);

        public void CheckAndProcessAllAuditFiles()
            => this.audioAuditService.CheckAndProcessAllAuditFiles();

        public bool IsViewVisible { get; set; }

        public CancellationToken CancellationToken => this.cancellation.Token;

        public async Task EvaluateAsync(Guid interviewId,
            Func<RecordingTarget> desiredTargetProvider,
            Func<Guid, Task<bool>> startRecordingAsync,
            CancellationToken cancellationToken)
        {
            if (desiredTargetProvider == null) throw new ArgumentNullException(nameof(desiredTargetProvider));
            if (startRecordingAsync == null) throw new ArgumentNullException(nameof(startRecordingAsync));

            while (true)
            {
                RecordingTarget target;

                await this.audioRecordingLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    // A start initiated by another evaluation is in flight (the lock is released while
                    // it dispatches to the main thread). That evaluation re-evaluates once it completes,
                    // so it will converge to the latest desired target — don't race a competing start here.
                    if (this.isAuditStarting)
                        return;

                    target = this.IsViewVisible ? desiredTargetProvider() : RecordingTarget.None;

                    // Already in the desired state.
                    if (this.currentRecordingTarget.Equals(target))
                        return;

                    // Stop the current recording when the target changed (group switch or leaving scope),
                    // so each applicable group is captured as its own audio file, like the general audio audit.
                    if (this.currentRecordingTarget.IsRecording)
                    {
                        this.currentRecordingTarget = RecordingTarget.None;
                        this.audioAuditService.StopAudioRecording(interviewId);
                    }

                    if (!target.IsRecording)
                        return;

                    // Reserve the start so concurrent evaluations and the teardown stop don't race the
                    // single underlying recorder while we dispatch to the main thread without the lock.
                    this.isAuditStarting = true;
                }
                finally
                {
                    this.audioRecordingLock.Release();
                }

                // Start outside the lock: the main-thread dispatch (which can navigate / show toasts on
                // permission failure) must not block stop/evaluate calls waiting on the recording lock.
                var started = await startRecordingAsync(interviewId).ConfigureAwait(false);

                // Reacquire without the cancellation token on purpose: this commit/cleanup must run
                // even if the view model is being disposed, so a recording started just above is always
                // either tracked or stopped (never leaked).
                await this.audioRecordingLock.WaitAsync().ConfigureAwait(false);
                try
                {
                    this.isAuditStarting = false;

                    if (!started)
                    {
                        // The start failed (e.g. missing permission, which already navigated away). Leave
                        // the target as None so a later evaluation can retry, and stop looping so we don't
                        // spin retrying a failing start while the view is still visible.
                        return;
                    }

                    if (this.IsViewVisible)
                    {
                        this.currentRecordingTarget = target;
                    }
                    else
                    {
                        // The view disappeared while we were starting; stop the recording we just started
                        // so we don't keep recording off-screen.
                        this.audioAuditService.StopAudioRecording(interviewId);
                    }
                }
                finally
                {
                    this.audioRecordingLock.Release();
                }

                // Loop to converge: navigation may have changed the desired target while the start was
                // running (other evaluations bailed out because the start guard was set).
            }
        }

        public async Task StopAsync(Guid interviewId)
        {
            await this.audioRecordingLock.WaitAsync().ConfigureAwait(false);
            try
            {
                // The user navigated back before this stop could run; EvaluateAsync will manage the
                // correct recording state — do not interfere.
                if (this.IsViewVisible)
                    return;

                if (this.currentRecordingTarget.IsRecording)
                {
                    this.currentRecordingTarget = RecordingTarget.None;
                    this.audioAuditService.StopAudioRecording(interviewId);
                }
            }
            finally
            {
                this.audioRecordingLock.Release();
            }
        }

        public void Cancel() => this.cancellation.Cancel();

        public void Dispose()
        {
            this.cancellation.Dispose();
            this.audioRecordingLock.Dispose();
        }
    }
}
