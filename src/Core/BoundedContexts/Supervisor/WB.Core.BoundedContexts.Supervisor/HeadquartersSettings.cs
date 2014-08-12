using System;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class HeadquartersSettings
    {
        public Uri LoginServiceEndpointUrl { get; private set; }
        public Uri UserChangedFeedUrl { get; set; }
        public Uri InterviewsFeedUrl { get; set; }
        public Uri QuestionnaireChangedFeedUrl { get; set; }
        public string QuestionnaireDetailsEndpoint { get; set; }
        public string AccessToken { get; set; }
        public Uri InterviewsPushUrl { get; set; }

        public HeadquartersSettings(
            Uri loginServiceEndpointUrl,
            Uri userChangedFeedUrl,
            Uri interviewsFeedUrl,
            string questionnaireDetailsEndpoint,
            string accessToken,
            Uri interviewsPushUrl,
            Uri questionnaireChangedFeedUrl)
        {
            this.LoginServiceEndpointUrl = loginServiceEndpointUrl;
            this.UserChangedFeedUrl = userChangedFeedUrl;
            this.InterviewsFeedUrl = interviewsFeedUrl;
            this.QuestionnaireDetailsEndpoint = questionnaireDetailsEndpoint;
            this.AccessToken = accessToken;
            this.InterviewsPushUrl = interviewsPushUrl;
            this.QuestionnaireChangedFeedUrl = questionnaireChangedFeedUrl;
        }
    }
}