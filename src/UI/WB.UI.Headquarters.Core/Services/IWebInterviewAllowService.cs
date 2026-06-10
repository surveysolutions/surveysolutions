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

        /// <summary>
        /// Key set in <c>HttpContext.Items</c> when access was granted via the prototype fast-path
        /// (before any interview entity is loaded). Presence of this key signals that password
        /// verification should be skipped — prototypes have no assignment password.
        /// </summary>
        public const string PrototypeAccessGrantedKey = "WebInterviewAllowService.PrototypeAccessGranted";

        void CheckWebInterviewAccessPermissions(string interviewId);
    }
}
