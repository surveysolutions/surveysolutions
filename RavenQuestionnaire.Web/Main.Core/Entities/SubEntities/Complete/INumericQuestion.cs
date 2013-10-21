namespace Main.Core.Entities.SubEntities.Complete
{
    public interface INumericQuestion 
    {
        bool IsInteger { get; set; }
        int? CountOfDecimalPlaces { get; set; }
    }
}