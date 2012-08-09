using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveysBrowseItem
    {

        public Guid Id { get; set; }

        public SurveyStatus Status { get; set; }

        public string TemplateId { get; set; }

        public string Title { get; set; }

        public UserLight Responsible { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Dictionary<string, int> Statistics { get; set; }

        public SurveysBrowseItem()
        {
            this.Statistics=new Dictionary<string, int>();
        }

        public SurveysBrowseItem(Guid id, string templateId, string title, UserLight responsible, SurveyStatus status):this()
        {
            this.Id = id;
            this.EndDate = DateTime.Now;
            this.StartDate = DateTime.Now;
            this.TemplateId = templateId;
            this.Title = title;
            this.Responsible = responsible;
            this.Status = status;
        }
    }
}
