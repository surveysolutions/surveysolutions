using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Indexes;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    internal class QuestionnaireListViewFactory : IQuestionnaireListViewFactory
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireListViewItem> questionnaireListViewItemStorage; 

        public QuestionnaireListViewFactory(IQueryableReadSideRepositoryReader<QuestionnaireListViewItem> questionnaireListViewItemStorage)
        {
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
        }

        public QuestionnaireListView Load(QuestionnaireListInputModel input)
        {
            var count = questionnaireListViewItemStorage.Query(_ => FilterQuestionnaires(_, input).Count());

            var records = questionnaireListViewItemStorage.Query(_ =>
                FilterQuestionnaires(_, input)
                    .OrderUsingSortExpression(input.Order)
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList());
            records.ForEach(x => x.Owner = x.CreatedBy == input.ViewerId ? "you" : x.CreatorName);
         /*    this.indexAccessor.Query<QuestionnaireListViewItemSearchable, List<QuestionnaireListViewItem>>(indexName, queryable =>
            {
                var queryResult =
                    FilterQuestionnaires(input).OrderUsingSortExpression(input.Order)
                        .Skip((input.Page - 1)*input.PageSize)
                        .Take(input.PageSize)
                        .ToList();
                queryResult.ForEach(x => x.Owner = x.CreatedBy == input.ViewerId ? "you" : x.CreatorName);
                return queryResult;

            });*/
           return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                  items: records,
                  order: input.Order);
        }

        private IQueryable<QuestionnaireListViewItem> FilterQuestionnaires(IQueryable<QuestionnaireListViewItem> _,
            QuestionnaireListInputModel input)
        {
            var result = _.Where(x => x.IsDeleted == false);
            if (!string.IsNullOrEmpty(input.Filter))
            {
                result =
                    result.Where(
                        x => x.Title.StartsWith(input.Filter) || x.CreatorName.StartsWith(input.Filter));
            }

            if (input.IsAdminMode)
            {
                if (!input.IsPublic)
                {
                    result =
                        result.Where(
                            x =>
                                x.CreatedBy == input.ViewerId ||
                                x.SharedPersons.Any(person => person == input.ViewerId));
                }
            }
            else
            {
                if (input.IsPublic)
                    result = result.Where(x => x.IsPublic);
                else
                    result =
                        result.Where(
                            x =>
                                x.CreatedBy == input.ViewerId ||
                                x.SharedPersons.Any(person => person == input.ViewerId));
            }
            return result;
        }
    }
}