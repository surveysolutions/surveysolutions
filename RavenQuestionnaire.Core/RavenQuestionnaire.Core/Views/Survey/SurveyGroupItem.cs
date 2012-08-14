using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupItem
    {
        public string Title { get; set; }
        public Guid Id { get; set; }
        public UserLight Responsible { get; set; }
        public string TemplateId { get; set; }
        public SurveyStatus Status { get; set; }
        public Dictionary<string, string> FeatureadValue { get; set; }
        
        public SurveyGroupItem()
        {
            this.FeatureadValue = new Dictionary<string, string>();
        }

        public SurveyGroupItem(Guid id, string title, string templateId, SurveyStatus status, UserLight responsible):this()
        {
            this.Id = id;
            this.Title = title;
            this.TemplateId = templateId;
            this.Status = status;
            this.Responsible = responsible;
        }
    }
}
