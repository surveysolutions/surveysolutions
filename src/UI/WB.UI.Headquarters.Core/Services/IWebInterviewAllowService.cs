using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public interface IWebInterviewAllowService
    {
        /// <summary>
        /// Key used to cache the loaded <see cref="IStatefulInterview"/> in <c>HttpContext.Items</c>
        /// after a successful <see cref="CheckWebInterviewAccessPermissions"/> call, so that
        /// subsequent filter steps can reuse it without an extra repository round-trip.
        /// </summary>
        public const string CachedInterviewItemsKey = "WebInterviewAllowService.CachedInterview";

        void CheckWebInterviewAccessPermissions(string interviewId);
    }
}
