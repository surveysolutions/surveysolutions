using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyBrowseItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string TemplateId { get; set; }
        public SurveyStatus Status { get; set; }
        public UserLight Responsible { get; set; }
        public int UnAssignment { get; set; }
        public Dictionary<Guid, SurveyStatus> AllQuestionnaire { get; set; }
        public Dictionary<string, int> Statistics { get; set; }
        public Dictionary<string, string> FeatureadValue { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public SurveyBrowseItem()
        {
            this.AllQuestionnaire=new Dictionary<Guid, SurveyStatus>();
            this.Statistics = new Dictionary<string, int>();
            this.FeatureadValue=new Dictionary<string, string>();
        }

        public SurveyBrowseItem(Guid id, string title, string templateId, SurveyStatus status, UserLight responsible):this()
        {
            this.Id = id;
            this.Title = title;
            this.EndDate = DateTime.Now;
            this.StartDate = DateTime.Now;
            this.TemplateId = templateId;
            this.Status = status;
            this.Responsible = responsible;
        }
    }
}
