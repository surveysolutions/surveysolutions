using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireItemViewFactory: IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession;

        public QuestionnaireItemViewFactory(IVersionedReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        public QuestionnaireBrowseItem Load(QuestionnaireItemInputModel input)
        {
            //return documentGroupSession.GetById(input.QuestionnaireId);
            return
                documentGroupSession.GetById(input.QuestionnaireId, input.Version)/*.Query(
                    _ => _.Where(q => q.QuestionnaireId == input.QuestionnaireId).OrderByDescending(q=>q.Version).ToList().FirstOrDefault())*/;
        }
    }
}
