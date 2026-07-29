using System;

namespace WB.UI.WebTester.Services
{
    /// <summary>
    /// Thread-safe store for background import creation status entries,
    /// keyed by interviewId (unique per run).
    /// Extracted as a standalone service to avoid a circular DI dependency
    /// between TokenEviction and ImportQuestionnaireAndCreateInterviewService.
    /// </summary>
    public interface IImportStatusStore
    {
        /// <summary>Atomically inserts a Loading sentinel if the key does not exist. Returns true on success.</summary>
        bool TryInitialize(Guid interviewId);

        void Set(Guid interviewId, CreationResult result);
        CreationResult? Get(Guid interviewId);
        CreationResult? Remove(Guid interviewId);
    }
}
