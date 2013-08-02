using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireItemViewFactory: IViewFactory<QuestionnaireItemInputModel, QuestionnaireBrowseItem>
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession;

        public QuestionnaireItemViewFactory(IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        public QuestionnaireBrowseItem Load(QuestionnaireItemInputModel input)
        {
            //return documentGroupSession.GetById(input.QuestionnaireId);
            return
                documentGroupSession.Query(
                    _ => _.Where(q => q.QuestionnaireId == input.QuestionnaireId).ToList().FirstOrDefault());
        }
    }
}
