using System;

#nullable enable
namespace Main.Core.Entities.SubEntities
{
    public class QuestionnaireCategoricalOption
    {
        public string Title { get; set; } = String.Empty;
        public int Value { get; set; }
        public int? ParentValue { get; set; }
        public int[]? ValueWithParentValues { get; set; }
        
        public string? AttachmentName { get; set; }
    }
}
