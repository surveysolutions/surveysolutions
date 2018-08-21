using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public class WebInterviewConfig
    {
        public WebInterviewConfig()
        {
            this.CustomMessages = new Dictionary<WebInterviewUserMessages, string>();
        }

        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public bool Started { get; set; }
        public bool UseCaptcha { get; set; }
        public Dictionary<WebInterviewUserMessages, string> CustomMessages { get; set; }

        public static Dictionary<WebInterviewUserMessages, string> DefaultMessages => 
            new Dictionary<WebInterviewUserMessages, string> 
            {
                { WebInterviewUserMessages.FinishInterview, Enumerator.Native.Resources.WebInterview.FinishInterviewText },
                { WebInterviewUserMessages.Invitation,      Enumerator.Native.Resources.WebInterview.InvitationText },
                { WebInterviewUserMessages.ResumeInvitation,Enumerator.Native.Resources.WebInterview.Resume_InvitationText },
                { WebInterviewUserMessages.ResumeWelcome,   Enumerator.Native.Resources.WebInterview.Resume_WelcomeText },
                { WebInterviewUserMessages.SurveyName,      Enumerator.Native.Resources.WebInterview.SurveyFormatText },
                { WebInterviewUserMessages.WebSurveyHeader, Enumerator.Native.Resources.WebInterview.WebSurvey },
                { WebInterviewUserMessages.WelcomeText,     Enumerator.Native.Resources.WebInterview.WelcomeText }
            };
    }
}
