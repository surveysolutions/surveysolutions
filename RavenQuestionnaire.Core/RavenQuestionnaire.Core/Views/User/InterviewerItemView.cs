using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Statistics;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerItemView
    {
        public string QuestionnaireTitle { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<Guid, string> FeaturedQuestions { get; set; }
        public SurveyStatus Status { get; private set; }

        public InterviewerItemView(CompleteQuestionnaireBrowseItem item, Dictionary<Guid, string> featuredHeaders)
        {
            TemplateId = item.TemplateId;
            Status = item.Status;
            FeaturedQuestions = new Dictionary<Guid, string>();

            foreach(var kvp in featuredHeaders)
            {
                var featured = item.FeaturedQuestions.FirstOrDefault(q => q.PublicKey == kvp.Key);
                FeaturedQuestions.Add(kvp.Key, featured == null ? "" : featured.AnswerText);
            }
            QuestionnaireTitle = item.QuestionnaireTitle;
        }
    }
}
