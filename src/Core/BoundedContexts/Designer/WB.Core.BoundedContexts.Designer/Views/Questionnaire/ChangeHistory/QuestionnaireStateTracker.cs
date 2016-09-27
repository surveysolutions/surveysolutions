using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireStateTracker
    {
        public Dictionary<Guid, Guid?> Parents { get; set; } = new Dictionary<Guid, Guid?>();
        public Dictionary<Guid, string> QuestionsState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> GroupsState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> RosterState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> StaticTextState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> VariableState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> MacroState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> LookupState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> AttachmentState { get; set; } = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> TranslationState { get; set; } = new Dictionary<Guid, string>();
        public Guid CreatedBy { get; set; }

        public void RemoveCascadely(Guid groupId)
        {
            var idsToRemove = groupId.TreeToEnumerable(this.GetChildren).ToList();

            foreach (Guid id in idsToRemove)
            {
                this.Parents.Remove(id);
                this.QuestionsState.Remove(id);
                this.GroupsState.Remove(id);
                this.RosterState.Remove(id);
                this.StaticTextState.Remove(id);
                this.VariableState.Remove(id);
            }
        }

        private IEnumerable<Guid> GetChildren(Guid id) => this.Parents.Where(x => x.Value == id).Select(x => x.Key).ToList();
    }
}