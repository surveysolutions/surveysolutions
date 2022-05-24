using System;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{

    public interface IQuestionnaireInfoViewFactory
    {
        QuestionnaireInfoView? Load(QuestionnaireRevision questionnaireRevision, Guid? viewerId);
    }
}
