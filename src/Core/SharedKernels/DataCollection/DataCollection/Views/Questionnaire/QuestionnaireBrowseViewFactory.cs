using System.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireBrowseViewFactory : IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession;

        public QuestionnaireBrowseViewFactory(IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
            int count = this.documentGroupSession.Count();
            if (count == 0)
            {
                return new QuestionnaireBrowseView(
                    input.Page, input.PageSize, count, new QuestionnaireBrowseItem[0], string.Empty);
            }

            return this.documentGroupSession.Query(queryable =>
            {
                IQueryable<QuestionnaireBrowseItem> query = queryable;

                if (input.IsAdminMode.HasValue)
                {
                    if (input.IsOnlyOwnerItems)
                    {
                        query = query.Where(x => x.CreatedBy == input.CreatedBy);
                    }

                    if (!input.IsAdminMode.Value)
                    {
                        query = query.Where(x => !x.IsDeleted);
                    }

                    if (input.QuestionnaireId.HasValue)
                    {
                        query = query.Where(x => x.QuestionnaireId == input.QuestionnaireId.Value);
                    }

                    if (input.Version.HasValue)
                    {
                        query = query.Where(x => x.Version == input.Version.Value);
                    }

                    if (!string.IsNullOrEmpty(input.Filter))
                    {
#warning ReadLayer: ToList materialization because not supported by Raven
                        query = query.ToList().AsQueryable().Where(x => x.Title.ContainsIgnoreCaseSensitive(input.Filter));
                    }
                }

#warning ReadLayer: ToList materialization because not supported by Raven
                var queryResult = query.ToList().AsQueryable().OrderUsingSortExpression(input.Order);

                var questionnaireItems = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();


                return new QuestionnaireBrowseView(input.Page, input.PageSize, queryResult.Count(), questionnaireItems, input.Order);
            });
        }
    }
}