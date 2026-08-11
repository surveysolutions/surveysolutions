using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.BinaryData;
using WB.Enumerator.Native.WebInterview;

namespace WB.UI.Headquarters.Services.Impl
{
    public class AudioAuditAccessService : IAudioAuditAccessService
    {
        // Filename pattern: {interviewId}-audio-audit-yyyyMMdd_HHmmssfff.m4a
        private static readonly Regex TimestampPattern = new Regex(
            @"-audio-audit-(\d{8}_\d{9})\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IAuthorizedUser authorizedUser;
        private readonly IAudioAuditFileStorage audioAuditFileStorage;
        private readonly IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage;
        private readonly IReviewAllowedService reviewAllowedService;

        public AudioAuditAccessService(
            IAuthorizedUser authorizedUser,
            IAudioAuditFileStorage audioAuditFileStorage,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettingsStorage,
            IReviewAllowedService reviewAllowedService)
        {
            this.authorizedUser = authorizedUser;
            this.audioAuditFileStorage = audioAuditFileStorage;
            this.interviewerSettingsStorage = interviewerSettingsStorage;
            this.reviewAllowedService = reviewAllowedService;
        }

        public bool CanAccessAudioAudit(Guid interviewId)
        {
            if (!authorizedUser.IsAuthenticated) return false;
            if (authorizedUser.IsInterviewer) return false;

            try
            {
                reviewAllowedService.CheckIfAllowed(interviewId);
            }
            catch (InterviewAccessException)
            {
                return false;
            }

            if (!authorizedUser.IsSupervisor)
                return true;

            // Supervisor workspace setting must allow playback (defaults to false)
            var settings = interviewerSettingsStorage.GetById(AppSetting.InterviewerSettings);
            return settings.IsAllowSupervisorAudioAuditPlayback();

        }

        public async Task<IReadOnlyList<AudioAuditSegmentInfo>> GetAudioAuditSegmentsAsync(Guid interviewId)
        {
            if (!CanAccessAudioAudit(interviewId)) return null;

            var descriptors = await audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);

            var ordered = descriptors
                .Select(d => (descriptor: d, timestamp: ParseTimestamp(d.FileName)))
                .OrderBy(x => x.timestamp == null)
                .ThenBy(x => x.timestamp)
                .ThenBy(x => x.descriptor.FileName)
                .Select((x, index) => new AudioAuditSegmentInfo
                {
                    SegmentId = BuildOpaqueId(x.descriptor.FileName),
                    SequenceNumber = index + 1,
                    DeviceLocalStartTime = x.timestamp,
                    ContentType = x.descriptor.ContentType,
                })
                .ToList();

            return ordered;
        }

        public async Task<InterviewBinaryDataDescriptor> ResolveSegmentAsync(Guid interviewId, string segmentId)
        {
            if (!CanAccessAudioAudit(interviewId)) return null;

            var fileName = DecodeOpaqueId(segmentId);
            if (fileName == null) return null;

            var descriptors = await audioAuditFileStorage.GetBinaryFilesForInterview(interviewId);
            return descriptors.FirstOrDefault(d => d.FileName == fileName);
        }

        /// <summary>
        /// Parses the device-local timestamp string from a filename.
        /// Returns the raw timestamp string (yyyyMMdd_HHmmssfff) for sorting, or null when the filename
        /// does not match the expected pattern. A malformed filename must not prevent segment display.
        /// </summary>
        private static string ParseTimestamp(string fileName)
        {
            var match = TimestampPattern.Match(fileName);
            return match.Success ? match.Groups[1].Value : null;
        }

        private static string BuildOpaqueId(string fileName)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private static string DecodeOpaqueId(string opaqueId)
        {
            try
            {
                var padded = opaqueId.Replace('-', '+').Replace('_', '/');
                switch (padded.Length % 4)
                {
                    case 2: padded += "=="; break;
                    case 3: padded += "="; break;
                }
                var bytes = Convert.FromBase64String(padded);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }
    }
}
