using System.Collections.Generic;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace Main.Core.Entities.SubEntities
{
    public interface IValidatable : IQuestionnaireEntity
    {
        IList<ValidationCondition> ValidationConditions { get; set; }
    }
}
