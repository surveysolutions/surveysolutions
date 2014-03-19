using System;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.Api.Authentication;

namespace WB.UI.Headquarters.Api
{
    public class FeedController : ApiController
    {
        private readonly ISupervisorFeedService feedService;

        public FeedController(ISupervisorFeedService feedService)
        {
            if (feedService == null) throw new ArgumentNullException("feedService");
            this.feedService = feedService;
        }

        [HttpGet]
        [Route("api/feed/v0")]
        [BasicAuthentication]
        public object Index()
        {
            return feedService.GetEntry(User.Identity.Name);
        }
    }
}