using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RavenQuestionnaire.Core.Views.Statistics;

namespace Web.Supervisor.Models
{
    public class ApproveModel
    {
        public string TemplateId { get; set; }
        public Guid Id { get; set; }
        public string Comment { get; set; }
        public CompleteQuestionnaireStatisticView Statistic { get; set; }
    }
}