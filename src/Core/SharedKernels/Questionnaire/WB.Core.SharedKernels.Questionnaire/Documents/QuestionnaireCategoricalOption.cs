#nullable enable
namespace Main.Core.Entities.SubEntities
{
    public class QuestionnaireCategoricalOption
    {
        public string? Title { get; set; }
        public int? Value { get; set; }
        public int? ParentValue { get; set; }
        public int[]? ValueWithParentValues { get; set; }
    }
}
