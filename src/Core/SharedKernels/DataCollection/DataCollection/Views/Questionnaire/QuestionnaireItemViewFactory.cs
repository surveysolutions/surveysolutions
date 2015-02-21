using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireItemViewFactory: IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>
    {
        private readonly IReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession;

        public QuestionnaireItemViewFactory(IReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        public QuestionnaireBrowseItem Load(QuestionnaireItemInputModel input)
        {
            //return documentGroupSession.GetById(input.QuestionnaireId);
            return
                this.documentGroupSession.AsVersioned().Get(input.QuestionnaireId.FormatGuid(), input.Version)/*.Query(
                    _ => _.Where(q => q.QuestionnaireId == input.QuestionnaireId).OrderByDescending(q=>q.Version).ToList().FirstOrDefault())*/;
        }
    }
}
