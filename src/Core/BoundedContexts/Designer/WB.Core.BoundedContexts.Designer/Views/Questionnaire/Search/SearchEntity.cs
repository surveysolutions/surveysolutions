using System;
using Main.Core.Entities.Composite;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class SearchEntity
    {
        public string? Text { get; set; }
        public Guid EntityId { get; set; }
        public Guid QuestionnaireId { get; set; }
        public IComposite? Entity { get; set; }
        internal string? SearchText {get; set; }
    }
}
