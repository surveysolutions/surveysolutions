using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace CAPI.Android.Controls.QuestionnaireDetails.ScreenItems
{
    public interface IAnswerOnQuestionCommandService
    {
        void Execute(AnswerQuestionCommand command);
    }
}