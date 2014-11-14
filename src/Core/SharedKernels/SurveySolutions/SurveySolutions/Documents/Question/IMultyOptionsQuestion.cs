namespace Main.Core.Entities.SubEntities.Question
{
    public interface IMultyOptionsQuestion : IQuestion
    {
        bool AreAnswersOrdered { get; set; }

        int? MaxAllowedAnswers { get; set; }
    }
}