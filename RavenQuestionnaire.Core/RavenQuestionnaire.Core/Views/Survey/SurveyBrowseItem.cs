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
        public int UnAssigment { get; set; }
        public Dictionary<Guid, SurveyItem> Statistic { get; set; }
        public Dictionary<string, int> Grid { get; set; }
        public int Total { get; set; }

        public SurveyBrowseItem()
        {
            this.Grid = new Dictionary<string, int>();
            var statuses = SurveyStatus.GetAllStatuses().Select(s => s.Name).ToList();
            statuses.Insert(0, "Total");
            statuses.Insert(1, "Unassigned");
            foreach (var statuse in statuses)
                this.Grid.Add(statuse, 0);
        }

        public SurveyBrowseItem(string id, string title, int unAssigment, Dictionary<Guid, SurveyItem> statistic, int total):this()
        {
            this.Id = id;
            this.Title = title;
            this.UnAssigment = unAssigment;
            this.Total = statistic.Count;
            this.Grid["Total"] = this.Total;
            this.Grid["Unassigned"] = unAssigment;
            this.Statistic = statistic;
            var items = statistic.Values.Where(t=>t.Responsible!=null).GroupBy(r => r.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToList();
            foreach (var item in items)
            {
                if (Grid.ContainsKey(item.Status.Name))
                    Grid[item.Status.Name] += 1;
                else
                    Grid.Add(item.Status.Name, 1);
            }
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
