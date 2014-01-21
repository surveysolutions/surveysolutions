namespace Main.Core.Entities.SubEntities.Question
{
    public interface ITextListQuestion : IQuestion
    {
        int? MaxAnswerCount { get; set; }
    }
}