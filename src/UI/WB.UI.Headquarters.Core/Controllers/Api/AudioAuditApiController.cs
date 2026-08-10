#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers.Api
{
    [Authorize]
    [Route("api/audioaudit")]
    public class AudioAuditApiController : ControllerBase
    {
        private const string FallbackAudioMimeType = "audio/mp4";

        private readonly IAudioAuditAccessService audioAuditAccessService;

        public AudioAuditApiController(IAudioAuditAccessService audioAuditAccessService)
        {
            this.audioAuditAccessService = audioAuditAccessService;
        }

        /// <summary>
        /// Returns ordered audio-audit segment metadata for an interview.
        /// Returns 404 for missing interview, unauthorized access, or ineligible user —
        /// this prevents the API from disclosing whether an interview or recording exists.
        /// </summary>
        [HttpGet("{interviewId:guid}/info")]
        public async Task<IActionResult> GetAudioAuditInfo(Guid interviewId)
        {
            var segments = await audioAuditAccessService.GetAudioAuditSegmentsAsync(interviewId);
            if (segments == null) return NotFound();

            return Ok(new
            {
                hasAudioAudit = segments.Count > 0,
                segments,
            });
        }

        /// <summary>
        /// Returns audio-audit segment binary data for an interview with HTTP byte-range support.
        /// Returns 404 for missing interview, unauthorized access, invalid segment, or missing file.
        /// Content-Disposition is inline; no download filename is exposed.
        /// </summary>
        [HttpGet("{interviewId:guid}/segment/{segmentId}")]
        public async Task<IActionResult> GetAudioAuditSegment(Guid interviewId, string segmentId)
        {
            var descriptor = await audioAuditAccessService.ResolveSegmentAsync(interviewId, segmentId);
            if (descriptor == null) return NotFound();

            var data = await descriptor.GetData();
            if (data == null || data.Length == 0) return NotFound();

            var mimeType = GetSafeAudioMimeType(descriptor.ContentType);

            // File() with enableRangeProcessing=true lets ASP.NET Core handle Range headers
            // correctly for browser seeking without requiring true streaming.
            return File(data, mimeType, enableRangeProcessing: true);
        }

        private static string GetSafeAudioMimeType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return FallbackAudioMimeType;

            // Only allow audio/* MIME types to prevent serving arbitrary content as audio.
            var lower = contentType.Trim().ToLowerInvariant();
            if (lower.StartsWith("audio/")) return lower;

            return FallbackAudioMimeType;
        }
    }
}
