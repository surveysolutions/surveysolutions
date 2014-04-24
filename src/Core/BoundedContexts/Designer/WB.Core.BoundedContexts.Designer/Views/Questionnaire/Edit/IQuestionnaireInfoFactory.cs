using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface IQuestionnaireInfoFactory
    {
        NewEditGroupView GetGroupEditView(string questionnaireId, Guid groupId);
        NewEditRosterView GetRosterEditView(string questionnaireId, Guid rosterId);
        NewEditQuestionView GetQuestionEditView(string questionnaireId, Guid questionId);
    }
}