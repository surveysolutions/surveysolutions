using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireItemViewFactory: IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>
    {
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires;

        public QuestionnaireItemViewFactory(IReadSideRepositoryReader<QuestionnaireBrowseItem> questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public QuestionnaireBrowseItem Load(QuestionnaireItemInputModel input)
        {
            var result = this.questionnaires.AsVersioned().Get(input.QuestionnaireId.FormatGuid(), input.Version);
            return result;
        }
    }
}
