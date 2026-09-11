using System;
using System.Threading;

namespace WB.UI.WebTester.Infrastructure
{
    /// <summary>
    /// Ambient per-async-call-chain holder for the current interview's ID.
    /// Set once in <c>WebTesterController.Run</c> (before starting the background import task)
    /// and in <c>WebTesterSessionAuthorizeAttribute</c> (for subsequent HTTP requests).
    /// Uses <see cref="AsyncLocal{T}"/> so the value flows naturally into child Tasks,
    /// async continuations and Refit DelegatingHandler chains — without coupling callers
    /// to a scoped service.
    /// </summary>
    public static class DesignerJwtContext
    {
        private static readonly AsyncLocal<Guid?> _interviewId = new();

        /// <summary>
        /// The <c>interviewId</c> (unique per test run) to use when looking up the
        /// delegated JWT or user context for outbound Designer API calls.
        /// <c>null</c> when no interview session is active on the current execution context.
        /// </summary>
        public static Guid? InterviewId
        {
            get => _interviewId.Value;
            set => _interviewId.Value = value;
        }
    }
}

