using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Views.TakeNew
{
    public interface ITakeNewInterviewViewFactory
    {
        TakeNewInterviewView Load(TakeNewInterviewInputModel input);
    }

    public class TakeNewInterviewViewFactory : ITakeNewInterviewViewFactory
    {
        private readonly IQuestionnaireStorage questionnaireStorage;

        public TakeNewInterviewViewFactory(IQuestionnaireStorage questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = this.questionnaireStorage.GetQuestionnaireDocument(input.QuestionnaireId,
                input.QuestionnaireVersion.Value);

            return new TakeNewInterviewView(questionnaire, input.QuestionnaireVersion.Value);
        }
    }
}