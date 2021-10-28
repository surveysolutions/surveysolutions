using System;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface ILinkedQuestion : IQuestion
    {
        Guid? LinkedToRosterId { get; set; }
        Guid? LinkedToQuestionId { get; set; }
        string? FilterExpression { get; set; }
    }
}