using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IQuestionnaireInfoViewFactory
    {
        private readonly IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage;

        public QuestionnaireInfoViewFactory(IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public QuestionnaireInfoView Load(string questionnaireId)
        {
            return this.questionnaireStorage.GetById(questionnaireId);
        }
    }
}