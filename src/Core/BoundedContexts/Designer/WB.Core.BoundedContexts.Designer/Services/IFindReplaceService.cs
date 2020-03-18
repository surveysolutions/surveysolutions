using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IFindReplaceService
    {
        IEnumerable<QuestionnaireEntityReference> FindAll(QuestionnaireRevision questionnaireId, string searchFor, bool matchCase, bool matchWholeWord, bool useRegex);

        IEnumerable<QuestionnaireEntityReference> FindAll(QuestionnaireDocument questionnaire, string searchFor, bool matchCase, bool matchWholeWord, bool useRegex);

        int ReplaceTexts(QuestionnaireRevision questionnaireId, Guid responsibleId, string searchFor, string replaceWith, bool matchCase, bool matchWholeWord, bool useRegex);

        int ReplaceTexts(QuestionnaireDocument questionnaire, Guid responsibleId, string searchFor, string replaceWith, bool matchCase, bool matchWholeWord, bool useRegex);
    }
}
