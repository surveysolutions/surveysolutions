using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo
{
    public class QuestionnaireInfoViewFactory : IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView>
    {
        private readonly IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage;

        public QuestionnaireInfoViewFactory(IReadSideRepositoryReader<QuestionnaireInfoView> questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public QuestionnaireInfoView Load(QuestionnaireInfoViewInputModel input)
        {
            return this.questionnaireStorage.GetById(input.QuestionnaireId);
        }
    }
}