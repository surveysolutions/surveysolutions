namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public interface ILinkedQuestion : IQuestion
    {
        string? LinkedTo { get; set; }
        string? FilterExpression { get; set; }
    }
}