namespace Main.Core.Entities.SubEntities.Question
{
    public interface INumericQuestion : IQuestion
    {
        bool IsInteger { get; set; }
        int? CountOfDecimalPlaces { get; set; }
        bool UseFormatting { get; set; }
    }
}