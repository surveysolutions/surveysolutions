using System.Collections.Generic;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface IValidatable : IQuestionnaireEntity
    {
        IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
