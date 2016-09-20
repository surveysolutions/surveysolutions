using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public interface IQuestionnaireHistory
    {
        void Write(QuestionnaireCommand questionnaireCommand);
    }
}