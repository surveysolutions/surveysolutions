using System;
using System.Configuration;
using WB.Core.BoundedContexts.Supervisor;

namespace WB.UI.Supervisor
{
    public class HeadquartersSettings : ConfigurationSection, IHeadquartersSettings
    {
        [ConfigurationProperty("baseUrl")]
        public Uri BaseHqUrl
        {
            get { return (Uri)this["baseUrl"]; }
        }

        [ConfigurationProperty("loginServiceEndpoint")]
        public Uri LoginServiceEndpointUrl
        {
            get { return new Uri(this.BaseHqUrl, (Uri)this["loginServiceEndpoint"]); }
        }
        [ConfigurationProperty("userChangedFeed")]
        public Uri UserChangedFeedUrl
        {
            get { return new Uri(this.BaseHqUrl, (Uri)this["userChangedFeed"]); }
        }
        [ConfigurationProperty("interviewsFeed")]
        public Uri InterviewsFeedUrl
        {
            get { return new Uri(this.BaseHqUrl, (Uri)this["interviewsFeed"]); }
        }
        [ConfigurationProperty("questionnaireChangedFeed")]
        public Uri QuestionnaireChangedFeedUrl
        {
            get { return new Uri(this.BaseHqUrl, (Uri)this["questionnaireChangedFeed"]); }
        }
        [ConfigurationProperty("questionnaireDetailsEndpoint")]
        public string QuestionnaireDetailsEndpoint
        {
            get { return new Uri(this.BaseHqUrl, (string)this["questionnaireDetailsEndpoint"]).ToString(); }
        }
        [ConfigurationProperty("accessToken")]
        public string AccessToken
        {
            get { return (string)this["accessToken"]; }
        }
        [ConfigurationProperty("interviewsPushEndpoint")]
        public Uri InterviewsPushUrl
        {
            get { return new Uri(this.BaseHqUrl, (Uri)this["interviewsPushEndpoint"]); }
        }
        [ConfigurationProperty("filePushEndpoint")]
        public Uri FilePushUrl
        {
            get { return new Uri(this.BaseHqUrl, (Uri)this["filePushEndpoint"]); }
        }
    }
}