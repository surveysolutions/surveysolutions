using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Events.Questionnaire
{
    public class FullGroupDataEvent : QuestionnaireActiveEvent
    {
        public string ConditionExpression { get; set; }
        public string GroupText { get; set; }
        public string VariableName { get; set; }
        public string Description { get; set; }
        public bool HideIfDisabled { get; set; }
    }
}