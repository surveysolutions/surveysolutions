using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status.Processing
{
    public class StatusProcessView
    {
        public string Id { get; set; }//?
        public string Title { get; set; }

        public Guid PublicKey { set; get; }

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
            string questionnaireId,
            Guid publicKey)
            : this()
        {
            this.Id = IdUtil.ParseId(id); 
            this.Title = title;
            this.IsVisible = isVisible;
            this.PublicKey = publicKey;
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
