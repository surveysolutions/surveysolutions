using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    public class AudioAuditRecordingExecutor : IAudioAuditRecordingExecutor
    {
        // Defensive cap so a runaway desiredTargetProvider that keeps toggling faster than a start
        // completes cannot spin start/stop on the recorder indefinitely.
        private const int MaxConvergeIterations = 10;

        private readonly IAudioAuditService audioAuditService;
        private readonly ILogger logger;

        private bool isAuditStarting;
        private RecordingTarget currentRecordingTarget = RecordingTarget.None;
        private readonly SemaphoreSlim audioRecordingLock = new SemaphoreSlim(1, 1);
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();

        // Disposal is coordinated with in-flight fire-and-forget operations: the synchronization
        // primitives are only disposed once no EvaluateAsync/StopAsync is running, so a running
        // operation can never hit ObjectDisposedException on the lock or the cancellation source.
        private readonly object disposeGate = new object();
        private int activeOperations;
        private bool isDisposed;

        public AudioAuditRecordingExecutor(IAudioAuditService audioAuditService, ILogger logger)
        {
            this.audioAuditService = audioAuditService ?? throw new ArgumentNullException(nameof(audioAuditService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            if (!this.TryBeginOperation())
                return;

            try
            {
                var iterations = 0;
                while (true)
                {
                    if (++iterations > MaxConvergeIterations)
                    {
                        // The desired target kept changing faster than a start could complete. Stop looping
                        // to avoid thrashing the recorder; a later evaluation (navigation, appear) reconverges.
                        this.logger.Warn(
                            $"Audio audit recording convergence exceeded {MaxConvergeIterations} iterations for interview {interviewId}; aborting to avoid thrashing the recorder.");
                        return;
                    }

                    RecordingTarget target;

                    if (!await this.TryAcquireLockAsync(cancellationToken).ConfigureAwait(false))
                        return;
                    try
                    {
                        if (cancellationToken.IsCancellationRequested || this.IsCancelled)
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
                            this.logger.Debug($"Audio audit stopping current recording for interview {interviewId} (target changed).");
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
                    bool started;
                    try
                    {
                        started = await startRecordingAsync(interviewId).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        // A failure here (e.g. the recorder throwing) must be treated like started == false:
                        // reset the start guard below and stop looping, otherwise isAuditStarting would stay
                        // set and every future evaluation would bail out, silently disabling recording.
                        this.logger.Error($"Audio audit recording failed to start for interview {interviewId}.", exception);
                        started = false;
                    }

                    // Reacquire without the cancellation token on purpose: this commit/cleanup must run
                    // even if the view model is being disposed, so a recording started just above is always
                    // either tracked or stopped (never leaked).
                    if (!await this.TryAcquireLockAsync(CancellationToken.None).ConfigureAwait(false))
                    {
                        // The executor was disposed while starting. Best-effort stop so we don't leak a
                        // recording that was just started, then bail (there is nothing left to track).
                        if (started)
                        {
                            this.logger.Debug($"Audio audit executor disposed while starting; stopping recording for interview {interviewId}.");
                            this.audioAuditService.StopAudioRecording(interviewId);
                        }
                        return;
                    }
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

                        // Cancellation/disposal or the view disappearing may have happened while we were
                        // starting (that dispatch runs without the lock). Do not keep a recording that a
                        // torn-down or hidden screen no longer wants: stop it and bail.
                        if (cancellationToken.IsCancellationRequested || this.IsCancelled || !this.IsViewVisible)
                        {
                            this.logger.Debug($"Audio audit stopping just-started recording for interview {interviewId} (screen hidden or cancelled).");
                            this.audioAuditService.StopAudioRecording(interviewId);
                            return;
                        }

                        this.currentRecordingTarget = target;
                    }
                    finally
                    {
                        this.audioRecordingLock.Release();
                    }

                    // Loop to converge: navigation may have changed the desired target while the start was
                    // running (other evaluations bailed out because the start guard was set).
                }
            }
            finally
            {
                this.EndOperation();
            }
        }

        public async Task StopAsync(Guid interviewId)
        {
            if (!this.TryBeginOperation())
                return;

            try
            {
                if (!await this.TryAcquireLockAsync(CancellationToken.None).ConfigureAwait(false))
                    return;
                try
                {
                    // The user navigated back before this stop could run; EvaluateAsync will manage the
                    // correct recording state — do not interfere.
                    if (this.IsViewVisible)
                        return;

                    if (this.currentRecordingTarget.IsRecording)
                    {
                        this.logger.Debug($"Audio audit stopping recording for interview {interviewId} on teardown.");
                        this.currentRecordingTarget = RecordingTarget.None;
                        this.audioAuditService.StopAudioRecording(interviewId);
                    }
                }
                finally
                {
                    this.audioRecordingLock.Release();
                }
            }
            finally
            {
                this.EndOperation();
            }
        }

        public void Cancel()
        {
            // The owning view model can be disposed more than once (e.g. NavigateBack disposes it and
            // MvvmCross disposes it again on activity destroy), so guard against cancelling a source
            // that has already been disposed.
            lock (this.disposeGate)
            {
                if (this.isDisposed)
                    return;

                try
                {
                    this.cancellation.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore: cancellation source may already be in a terminal state.
                }
            }
        }

        public void Dispose()
        {
            lock (this.disposeGate)
            {
                if (this.isDisposed)
                    return;

                this.isDisposed = true;

                try
                {
                    this.cancellation.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Ignore: cancellation source may already be in a terminal state.
                }

                // Only dispose the synchronization primitives once no fire-and-forget operation is
                // running. If one is still in flight it will dispose them when it finishes (EndOperation),
                // so a running operation can never hit ObjectDisposedException on the lock/CTS.
                if (this.activeOperations == 0)
                    this.DisposeResources();
            }
        }

        private bool IsCancelled
        {
            get
            {
                lock (this.disposeGate)
                {
                    return this.isDisposed || this.cancellation.IsCancellationRequested;
                }
            }
        }

        private bool TryBeginOperation()
        {
            lock (this.disposeGate)
            {
                if (this.isDisposed)
                    return false;

                this.activeOperations++;
                return true;
            }
        }

        private void EndOperation()
        {
            lock (this.disposeGate)
            {
                this.activeOperations--;

                if (this.isDisposed && this.activeOperations == 0)
                    this.DisposeResources();
            }
        }

        // Must be called under disposeGate.
        private void DisposeResources()
        {
            try
            {
                this.cancellation.Dispose();
            }
            catch
            {
                // Ignore: best-effort cleanup.
            }

            try
            {
                this.audioRecordingLock.Dispose();
            }
            catch
            {
                // Ignore: best-effort cleanup.
            }
        }

        private async Task<bool> TryAcquireLockAsync(CancellationToken cancellationToken)
        {
            try
            {
                await this.audioRecordingLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (OperationCanceledException)
            {
                // Cancelled while waiting for the lock (screen torn down): abort cleanly without mutating
                // any recording state, rather than surfacing an exception to the fire-and-forget caller.
                return false;
            }
            catch (ObjectDisposedException)
            {
                // The executor was disposed concurrently; abort the operation.
                return false;
            }
        }
    }
}
