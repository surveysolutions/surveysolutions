using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public interface IQuestionnaireListViewFactory
    {
        QuestionnaireListView Load(QuestionnaireListInputModel input);
        IReadOnlyCollection<QuestionnaireListViewItem> GetUserQuestionnaires(Guid userId, bool isAdmin, int pageIndex = 1, int pageSize = 128);
    }
}