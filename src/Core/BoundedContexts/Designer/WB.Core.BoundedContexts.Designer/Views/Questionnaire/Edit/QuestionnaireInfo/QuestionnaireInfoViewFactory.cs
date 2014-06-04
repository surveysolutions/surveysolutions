using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage;
        private readonly IReadSideRepositoryReader<QuestionnaireSharedPersons> sharedPersons;

        public QuestionnaireInfoViewFactory(IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage,
            IReadSideRepositoryReader<QuestionnaireSharedPersons> sharedPersons)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.sharedPersons = sharedPersons;
        }

        public QuestionnaireInfoView Load(string questionnaireId)
        {
            QuestionnaireInfoView questionnaireInfoView = this.questionnaireStorage.GetById(questionnaireId);

            QuestionnaireSharedPersons questionnaireSharedPersons = sharedPersons.GetById(questionnaireId);
            if (questionnaireSharedPersons != null)
            {
                questionnaireInfoView.SharedPersons = questionnaireSharedPersons.SharedPersons;
            }
            
            return questionnaireInfoView;
        }
    }
}