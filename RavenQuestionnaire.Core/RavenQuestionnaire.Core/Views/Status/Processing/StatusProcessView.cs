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

        public List<FlowRule> FlowRules { get; set; }

        public SurveyStatus DefaultIfNoConditions { set; get; }


        public StatusProcessView()
        {
        }

        public StatusProcessView(string id, 
            string title, bool isVisible, 
            List<FlowRule> flowRules,
            SurveyStatus defaultIfNoConditions,
            string questionnaireId):this()
        {
            this.Id = IdUtil.ParseId(id); 
            this.Title = title;
            this.IsVisible = isVisible;
            QuestionnaireId = questionnaireId;
            FlowRules = flowRules;
            DefaultIfNoConditions = defaultIfNoConditions;
        }
        public static StatusProcessView New()
        {
            return new StatusProcessView();
        }
    }
}
