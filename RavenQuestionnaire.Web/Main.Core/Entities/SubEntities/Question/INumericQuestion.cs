namespace Main.Core.Entities.SubEntities.Question
{
    public interface INumericQuestion 
    {
        bool IsInteger { get; set; }
        int? CountOfDecimalPlaces { get; set; }
    }
}