using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public interface IValidatable : IQuestionnaireEntity
    {
        IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
