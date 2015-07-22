using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireBrowseViewFactory : IQuestionnaireBrowseViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> reader;

        public QuestionnaireBrowseViewFactory(IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> reader)
        {
            this.reader = reader;
        }

        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
            int count = this.reader.Count();
            if (count == 0)
            {
                return new QuestionnaireBrowseView(
                    input.Page, input.PageSize.GetValueOrDefault(), count, new QuestionnaireBrowseItem[0], string.Empty);
            }

            return this.reader.Query(queryable =>
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
                        query = query.Where(x => x.Title.ContainsIgnoreCaseSensitive(input.Filter));
                    }
                }

                var queryResult = query.OrderUsingSortExpression(input.Order);

                IQueryable<QuestionnaireBrowseItem> pagedResults = queryResult;

                if (input.PageSize.HasValue)
                {
                    pagedResults = queryResult.Skip((input.Page - 1) * input.PageSize.Value).Take(input.PageSize.Value);
                }


                return new QuestionnaireBrowseView(input.Page, input.PageSize, queryResult.Count(), pagedResults.ToList(), input.Order);
            });
        }
    }
}