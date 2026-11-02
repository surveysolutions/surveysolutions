using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;

namespace WB.UI.Headquarters.Services
{
    public interface IAudioAuditAccessService
    {
        /// <summary>
        /// Returns true if the current user is authorized to access audio-audit recordings for the specified interview.
        /// Returns false for unauthorized access, unknown interviews, or ineligible users; never throws.
        /// </summary>
        bool CanAccessAudioAudit(Guid interviewId);

        /// <summary>
        /// Returns ordered audio-audit segment descriptors for the interview, or null if access is denied.
        /// </summary>
        Task<IReadOnlyList<AudioAuditSegmentInfo>> GetAudioAuditSegmentsAsync(Guid interviewId);

        /// <summary>
        /// Resolves an opaque segment identifier to the actual file descriptor within the authorized interview.
        /// Returns null if access is denied or the segment is not found.
        /// </summary>
        Task<InterviewBinaryDataDescriptor> ResolveSegmentAsync(Guid interviewId, string segmentId);
    }
}
