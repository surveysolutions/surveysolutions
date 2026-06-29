using System;

namespace WB.Core.BoundedContexts.Interviewer
{
    /// <summary>
    /// Business rules that decide whether audio should be recorded for a given
    /// questionnaire entity, based on the whole-interview Audio Audit flag and the
    /// selective interview-level Audio Audit scope.
    /// </summary>
    public static class AudioAuditScopeRules
    {
        public static bool ShouldRecord(bool? isAudioRecordingEnabled, Guid[] audioAuditScope, Guid? currentEntityId)
        {
            // Whole-interview Audio Audit: record everything and ignore the scope.
            if (isAudioRecordingEnabled == true)
                return true;

            // Audio Audit disabled and empty scope: do not record.
            if (audioAuditScope == null || audioAuditScope.Length == 0)
                return false;

            if (currentEntityId == null)
                return false;

            // Scope selection is explicit and is not inherited: recording is active
            // only while the currently navigated entity is itself in the scope.
            return Array.IndexOf(audioAuditScope, currentEntityId.Value) >= 0;
        }
    }
}
