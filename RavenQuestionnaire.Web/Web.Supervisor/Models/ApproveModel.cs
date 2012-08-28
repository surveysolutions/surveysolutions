using System;
using RavenQuestionnaire.Core.Views.Statistics;

namespace Web.Supervisor.Models
{
    public class ApproveModel
    {
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public CompleteQuestionnaireStatisticView Statistic { get; set; }
    }
}