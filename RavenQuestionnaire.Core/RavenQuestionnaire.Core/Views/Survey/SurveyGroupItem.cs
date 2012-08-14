using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupItem
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public UserLight Responsible { get; set; }
        public string TemplateId { get; set; }
        public SurveyStatus Status { get; set; }
        public Dictionary<Guid, string> FeatureadValue { get; set; }
        
        private SurveyGroupItem()
        {
            this.FeatureadValue = new Dictionary<Guid, string>();
        }

        public SurveyGroupItem(CompleteQuestionnaireBrowseItem it, Dictionary<Guid, string> headers):this()
        {
            this.Title = it.QuestionnaireTitle;
            this.Id = it.CompleteQuestionnaireId;
            this.Responsible = it.Responsible;
            this.TemplateId = it.TemplateId;
            this.Status = it.Status;
            foreach (var header in headers)
            {
                var question = it.FeaturedQuestions.FirstOrDefault(t=>t.PublicKey==header.Key);
                this.FeatureadValue.Add(header.Key, question != null ? question.AnswerValue : string.Empty);
            }
        }
    }
}
