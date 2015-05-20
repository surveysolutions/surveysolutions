using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Indexes;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    internal class QuestionnaireListViewFactory : IQuestionnaireListViewFactory
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public QuestionnaireListViewFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public QuestionnaireListView Load(QuestionnaireListInputModel input)
        {
            var indexName = typeof(DesignerReportQuestionnaireListViewItem).Name;
            var count =
                this.indexAccessor.Query<QuestionnaireListViewItemSearchable, int>(indexName, queryable => this.FilterQuestionnaires(queryable, input).Count());
           var records=
             this.indexAccessor.Query<QuestionnaireListViewItemSearchable, List<QuestionnaireListViewItem>>(indexName, queryable =>
            {
                var queryResult =
                    FilterQuestionnaires(queryable, input).OrderUsingSortExpression(input.Order)
                        .Skip((input.Page - 1)*input.PageSize)
                        .Take(input.PageSize)
                        .ToList();
                queryResult.ForEach(x => x.Owner = x.CreatedBy == input.ViewerId ? "you" : x.CreatorName);
                return queryResult;

            });
           return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                  items: records,
                  order: input.Order);
        }

        private IQueryable<QuestionnaireListViewItem> FilterQuestionnaires(IQueryable<QuestionnaireListViewItemSearchable> questionnaire,
            QuestionnaireListInputModel input)
        {
            var result = questionnaire;
            result = result.Where(x => x.IsDeleted == false);
            if (!string.IsNullOrEmpty(input.Filter))
            {
                result = result.Where(x => x.TitleIndexed.StartsWith(input.Filter) || x.CreatorName.StartsWith(input.Filter));
            }

            if (input.IsAdminMode)
            {
                if (!input.IsPublic)
                {
                    result = result.Where(x => x.CreatedBy == input.ViewerId || x.SharedPersons.Any(person => person == input.ViewerId));
                }
            }
            else
            {
                result = result.Where(x =>!x.IsDeleted);
                if (input.IsPublic)
                    result = result.Where(x => x.IsPublic);
                else
                    result = result.Where(x => x.CreatedBy == input.ViewerId || x.SharedPersons.Any(person => person == input.ViewerId));
            }
            
            return result;
        }
    }
}