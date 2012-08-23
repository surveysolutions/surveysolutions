using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Survey
{
    
    public class SurveyBrowseItem
    {
        public string Title { get; set; }
        public string Id { get; set; }
        public int Unassigned { get; set; }
        public Dictionary<Guid, SurveyItem> Statistic { get; set; }
        public Dictionary<string, int> Grid { get; set; }
        public int Total { get; set; }
        public int Initial { get; set; }
        public int Error { get; set; }
        public int Complete { get; set; }

        public SurveyBrowseItem()
        {
            this.Grid = new Dictionary<string, int>();
            var statuses = SurveyStatus.GetAllStatuses().Select(s => s.Name).ToList();
            statuses.Insert(0, "Total");
            statuses.Insert(1, "Unassigned");
            foreach (var statuse in statuses)
                this.Grid.Add(statuse, 0);
        }

        public SurveyBrowseItem(string id, string title, int unAssigment, Dictionary<Guid, SurveyItem> statistic, int total, int initial, int error, int completed):this()
        {
            this.Id = id;
            this.Title = title;
            this.Unassigned = unAssigment;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Complete = completed;
            this.Grid["Total"] = this.Total;
            this.Grid["Unassigned"] = unAssigment;
            this.Statistic = statistic;
            this.Grid["Initial"] = initial;
            this.Grid["Error"] = error;
            this.Grid["Complete"] = completed;
        }
    }


    public class SurveyItem
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid CompleteQuestionnaireId { get; set; }
        public SurveyStatus Status { get; set; }
        public UserLight Responsible { get; set; }

        public SurveyItem(DateTime startDate, DateTime endDate, Guid completeQuestionnaireId, SurveyStatus status, UserLight responsible)
        {
            this.Status = status;
            this.EndDate = endDate;
            this.StartDate = startDate;
            this.Responsible = responsible;
            this.CompleteQuestionnaireId = completeQuestionnaireId;
        }
    }
}
