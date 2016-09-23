using System;
using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Factories
{
    internal interface IQuestionnaireEntityFactory
    {
        IStaticText CreateStaticText(Guid entityId, string text, string attachmentName, string enablementCondition, bool hideIfDisabled, IList<ValidationCondition> validationConditions);
        IQuestion CreateQuestion(QuestionData question);
        IVariable CreateVariable(QuestionnaireVariable questionnaireVariable);
    }
}