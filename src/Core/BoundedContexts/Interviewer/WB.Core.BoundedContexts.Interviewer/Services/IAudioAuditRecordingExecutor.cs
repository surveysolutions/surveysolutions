using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Services
{
    /// <summary>
    /// Single interface for working with tablet audio audit: it exposes the low-level recording
    /// operations (via <see cref="IAudioAuditService"/>) together with the recording state machine
    /// extracted from the interview view model — the recording lock, the currently active
    /// <see cref="RecordingTarget"/>, the in-flight start guard and the cancellation that ties
    /// recording to the lifetime of the screen. The view model only supplies the policy (which target
    /// is desired and how to start a recording with permission handling) and toggles
    /// <see cref="IsViewVisible"/>; all concurrency lives here so it can be unit-tested in isolation.
    /// </summary>
    public interface IAudioAuditRecordingExecutor : IAudioAuditService, IDisposable
    {
        /// <summary>Whether the interview screen is currently visible. Set by the view model on appear/disappear.</summary>
        bool IsViewVisible { get; set; }

        /// <summary>Cancellation token tied to this executor, cancelled when the screen is torn down.</summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Converges the active recording to the desired target: stops the current recording on a
        /// target change (group switch / leaving scope) and starts the new one, reusing the same
        /// start/stop paths as the whole-interview audio audit. The recording lock is only held while
        /// reading/mutating recording state and while stopping; it is deliberately released around
        /// <paramref name="startRecordingAsync"/> (which may dispatch to the main thread and navigate /
        /// show toasts on a permission failure), so a teardown stop and subsequent evaluations are never
        /// blocked behind that UI work. Concurrent starts are prevented by an in-flight guard, and the
        /// loop re-evaluates afterwards to converge to the latest desired target if navigation changed
        /// it while the start was running.
        /// </summary>
        /// <param name="interviewId">Interview the recording belongs to.</param>
        /// <param name="desiredTargetProvider">Computes the desired recording target from the current flag/scope/navigation state.</param>
        /// <param name="startRecordingAsync">Starts the underlying recording (with permission handling); returns true only on success.</param>
        /// <param name="cancellationToken">Cancels the evaluation when the screen is torn down.</param>
        Task EvaluateAsync(Guid interviewId,
            Func<RecordingTarget> desiredTargetProvider,
            Func<Guid, Task<bool>> startRecordingAsync,
            CancellationToken cancellationToken);

        /// <summary>
        /// Stops any active recording under the recording lock so the state mutation stays atomic with
        /// <see cref="EvaluateAsync"/>. If the view has become visible again before the lock is acquired
        /// (user navigated back quickly), the stop is stale and leaves the recording that
        /// <see cref="EvaluateAsync"/> may have already started for the returning view untouched.
        /// </summary>
        Task StopAsync(Guid interviewId);

        /// <summary>Cancels the executor's <see cref="CancellationToken"/> on screen teardown.</summary>
        void Cancel();
    }
}
