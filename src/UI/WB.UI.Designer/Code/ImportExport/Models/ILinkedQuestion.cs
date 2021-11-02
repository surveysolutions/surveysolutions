using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface ILinkedQuestion : IQuestion
    {
        string? LinkedTo { get; set; }
        string? FilterExpression { get; set; }
    }
}