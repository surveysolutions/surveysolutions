namespace Main.Core.Entities.SubEntities.Question
{
    public interface IMultiAnswerQuestion : IQuestion
    {
        int? MaxAnswerCount { get; set; }
    }
}