using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface ILinkedQuestion : IQuestion
    {
        Guid? LinkedToId { get; set; }
        string? FilterExpression { get; set; }
    }
}