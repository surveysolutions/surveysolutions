using System;
using System.Configuration;

namespace WB.Core.BoundedContexts.Supervisor
{
    public interface IHeadquartersSettings {
        Uri BaseHqUrl { get; }

        Uri LoginServiceEndpointUrl { get; }

        Uri UserChangedFeedUrl { get; }

        Uri InterviewsFeedUrl { get; }

        Uri QuestionnaireChangedFeedUrl { get; }

        string QuestionnaireDetailsEndpoint { get; }

        string AccessToken { get; }

        Uri InterviewsPushUrl { get; }

        Uri FilePushUrl { get; }
    }
}