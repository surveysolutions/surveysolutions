using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public interface IQuestionnaireChangeHistoryFactory
    {
        QuestionnaireChangeHistory Load(Guid id, int page, int pageSize);
    }
}