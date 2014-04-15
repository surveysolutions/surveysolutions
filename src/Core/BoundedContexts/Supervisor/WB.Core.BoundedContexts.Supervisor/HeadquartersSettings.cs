using System;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class HeadquartersSettings
    {
        public Uri LoginServiceEndpointUrl { get; private set; }
        public Uri UserChangedFeedUrl { get; set; }
        public Uri InterviewsFeedUrl { get; set; }

        public HeadquartersSettings(Uri loginServiceEndpointUrl, Uri userChangedFeedUrl, Uri interviewsFeed)
        {
            this.LoginServiceEndpointUrl = loginServiceEndpointUrl;
            this.UserChangedFeedUrl = userChangedFeedUrl;
            this.InterviewsFeedUrl = interviewsFeed;
        }
    }
}