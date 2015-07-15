using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
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
            var usedSortExpression = string.IsNullOrEmpty(input.Order) ? "LastEntryDate DESC" : input.Order;

            var records = questionnaireListViewItemStorage.Query(_ =>
                FilterQuestionnaires(_, input).Select(x => new QuestionnaireListViewItem()
                {
                    CreatedBy = x.CreatedBy,
                    CreationDate = x.CreationDate,
                    CreatorName = x.CreatorName,
                    IsDeleted = x.IsDeleted,
                    IsPublic = x.IsPublic,
                    LastEntryDate = x.LastEntryDate,
                    Owner = x.Owner,
                    PublicId = x.PublicId,
                    QuestionnaireId = x.QuestionnaireId,
                    Title = x.Title,
                    SharedPersons = x.SharedPersons
                })
                    .OrderUsingSortExpression(usedSortExpression)
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList());

            records.ForEach(x => x.Owner = x.CreatedBy == input.ViewerId ? "you" : x.CreatorName);
            return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                items: records,
                order: usedSortExpression);
        }

        private IQueryable<QuestionnaireListViewItem> FilterQuestionnaires(IQueryable<QuestionnaireListViewItem> _,
            QuestionnaireListInputModel input)
        {
            var result = _.Where(x => x.IsDeleted == false);
            if (!string.IsNullOrEmpty(input.Filter))
            {
                var filterLowerCase = input.Filter.Trim().ToLower();
                result =
                    result.Where(
                        x => x.Title.ToLower().Contains(filterLowerCase) || x.CreatorName.ToLower().Contains(filterLowerCase));
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