using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace Main.Core.Entities.SubEntities
{
    public interface IValidatable
    {
        IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
