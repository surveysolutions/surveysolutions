using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
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