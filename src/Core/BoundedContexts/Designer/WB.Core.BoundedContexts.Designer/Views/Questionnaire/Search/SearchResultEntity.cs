using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search
{
    public class SearchResultEntity
    {
        public string? Title { get; set; }
        public string? QuestionnaireTitle { get; set; }
        public Guid? FolderId { get; set; }
        public string? FolderName { get; set; }
        public Guid QuestionnaireId { get; set; }
        public Guid SectionId { get; set; }
        public Guid EntityId { get; set; }
        public string? EntityType { get; set; }
    }
}
