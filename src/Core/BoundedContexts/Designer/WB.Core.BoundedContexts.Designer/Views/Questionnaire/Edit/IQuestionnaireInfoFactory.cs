using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface IQuestionnaireInfoFactory
    {
        NewEditGroupView GetGroupEditView(string questionnaireId, Guid groupId);

        NewEditRosterView GetRosterEditView(string questionnaireId, Guid rosterId);

        NewEditQuestionView GetQuestionEditView(string questionnaireId, Guid questionId);

        NewEditStaticTextView GetStaticTextEditView(string questionnaireId, Guid staticTextId);

        List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string questionnaireId, Guid id);
    }
}