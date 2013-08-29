using Main.Core.Commands.Questionnaire.Completed;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public interface IAnswerOnQuestionCommandService
    {
        void Execute(SetAnswerCommand command);
    }
}