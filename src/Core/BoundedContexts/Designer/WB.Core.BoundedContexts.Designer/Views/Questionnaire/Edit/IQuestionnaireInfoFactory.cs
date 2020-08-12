using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public interface IQuestionnaireInfoFactory
    {
        NewEditGroupView? GetGroupEditView(QuestionnaireRevision questionnaireId, Guid groupId);

        NewEditRosterView? GetRosterEditView(QuestionnaireRevision questionnaireId, Guid rosterId);

        NewEditQuestionView? GetQuestionEditView(QuestionnaireRevision questionnaireId, Guid questionId);

        NewEditStaticTextView? GetStaticTextEditView(QuestionnaireRevision questionnaireId, Guid staticTextId);

        List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(QuestionnaireRevision questionnaireId, Guid id);

        List<DropdownEntityView>? GetQuestionsEligibleForNumericRosterTitle(QuestionnaireRevision questionnaireId, Guid rosterId, Guid rosterSizeQuestionId);

        VariableView? GetVariableEditView(QuestionnaireRevision questionnaireId, Guid variableId);

        Guid GetSectionIdForItem(QuestionnaireRevision questionnaireId, Guid? entityid);
    }
}
