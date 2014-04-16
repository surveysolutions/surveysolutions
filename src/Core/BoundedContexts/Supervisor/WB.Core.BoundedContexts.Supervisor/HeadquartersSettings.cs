using System;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class HeadquartersSettings
    {
        public Uri LoginServiceEndpointUrl { get; private set; }
        public Uri UserChangedFeedUrl { get; set; }
        public Uri InterviewsFeedUrl { get; set; }
        public string QuestionnaireDetailsEndpoint { get; set; }

        public HeadquartersSettings(Uri loginServiceEndpointUrl, 
            Uri userChangedFeedUrl, 
            Uri interviewsFeed,
            string questionnaireDetailsEndpoint)
        {
            this.LoginServiceEndpointUrl = loginServiceEndpointUrl;
            this.UserChangedFeedUrl = userChangedFeedUrl;
            this.InterviewsFeedUrl = interviewsFeed;
            this.QuestionnaireDetailsEndpoint = questionnaireDetailsEndpoint;
        }
    }
}