using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status.Processing
{
    public class StatusProcessView
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public bool IsVisible { get; set; }
        public string QuestionnaireId { get; set; }

        public Dictionary<Guid, FlowRule> FlowRules { get; set; }

        public bool IsDefaultStuck { get; set; }


        public StatusProcessView()
        {
        }

        public StatusProcessView(string id, 
            string title, bool isVisible,
            Dictionary<Guid, FlowRule> flowRules,
            bool IsDefaultStuck,
            string questionnaireId):this()
        {
            this.Id = IdUtil.ParseId(id); 
            this.Title = title;
            this.IsVisible = isVisible;
            QuestionnaireId = questionnaireId;
            FlowRules = flowRules;
            this.IsDefaultStuck = IsDefaultStuck;
        }
        public static StatusProcessView New()
        {
            return new StatusProcessView();
        }
    }
}
