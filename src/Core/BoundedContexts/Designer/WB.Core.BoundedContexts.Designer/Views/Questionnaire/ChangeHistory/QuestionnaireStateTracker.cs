using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireStateTracker
    {
        public QuestionnaireStateTracker()
        {
            QuestionsState = new Dictionary<Guid, string>();
            GroupsState = new Dictionary<Guid, string>();
            RosterState=new Dictionary<Guid, string>();
            StaticTextState = new Dictionary<Guid, string>();
            VariableState = new Dictionary<Guid, string>();
            MacroState = new Dictionary<Guid, string>();
            LookupState = new Dictionary<Guid, string>();
            AttachmentState = new Dictionary<Guid, string>();
            TranslationState = new Dictionary<Guid, string>();
        }

        public Dictionary<Guid, string> QuestionsState { get; set; }
        public Dictionary<Guid, string> GroupsState { get; set; }
        public Dictionary<Guid, string> RosterState { get; set; }
        public Dictionary<Guid, string> StaticTextState { get; set; }
        public Dictionary<Guid, string> VariableState { get; set; }
        public Dictionary<Guid, string> MacroState { get; set; }
        public Dictionary<Guid, string> LookupState { get; set; }
        public Dictionary<Guid, string> AttachmentState { get; set; }
        public Dictionary<Guid, string> TranslationState { get; set; }
        public Guid CreatedBy { get; set; }
    }
}