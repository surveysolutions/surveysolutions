using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface IQuestionnaireInfoFactory
    {
        NewEditGroupView GetGroupEditView(string questionnaireId, Guid groupId);

        NewEditRosterView GetRosterEditView(string questionnaireId, Guid rosterId);

        NewEditQuestionView GetQuestionEditView(string questionnaireId, Guid questionId);

        NewEditStaticTextView GetStaticTextEditView(string questionnaireId, Guid staticTextId);

        List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string questionnaireId, Guid id);

        List<DropdownEntityView> GetQuestionsEligibleForNumericRosterTitle(string questionnaireId, Guid rosterId, Guid rosterSizeQuestionId);

        VariableView GetVariableEditView(string questionnaireId, Guid variableId);
    }
}