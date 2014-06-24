using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal interface IQuestionDetailsViewMapper
    {
        QuestionDetailsView Map(IQuestion question, Guid parentGroupId);
    }
}