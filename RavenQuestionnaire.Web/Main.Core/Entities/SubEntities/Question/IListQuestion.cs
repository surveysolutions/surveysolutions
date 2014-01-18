namespace Main.Core.Entities.SubEntities.Question
{
    public interface IListQuestion : IQuestion
    {
        int? MaxAnswerCount { get; set; }
    }
}