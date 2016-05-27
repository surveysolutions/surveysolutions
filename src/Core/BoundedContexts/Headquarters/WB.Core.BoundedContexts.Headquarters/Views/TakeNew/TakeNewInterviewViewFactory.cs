using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Views.TakeNew
{
    public interface ITakeNewInterviewViewFactory
    {
        TakeNewInterviewView Load(TakeNewInterviewInputModel input);
    }

    public class TakeNewInterviewViewFactory : ITakeNewInterviewViewFactory
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public TakeNewInterviewViewFactory(IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(input.QuestionnaireId,
                input.QuestionnaireVersion.Value);

            return new TakeNewInterviewView(questionnaire, input.QuestionnaireVersion.Value);
        }
    }
}