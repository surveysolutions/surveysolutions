using System;
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

        public IReadOnlyCollection<QuestionnaireListViewItem> GetUserQuestionnaires(Guid userId, bool isAdmin, int pageIndex = 1, int pageSize = 128)
        {
            return questionnaireListViewItemStorage.Query(_ =>
                this.FilterByQuestionnaires(_, userId, isAdmin).Select(x => new QuestionnaireListViewItem()
                {
                    IsPublic = x.IsPublic,
                    LastEntryDate = x.LastEntryDate,
                    Owner = x.CreatorName,
                    QuestionnaireId = x.QuestionnaireId,
                    Title = x.Title,
                })
                    .Skip((pageIndex - 1)*pageSize)
                    .Take(pageSize)
                    .ToReadOnlyCollection());
        }

        private IQueryable<QuestionnaireListViewItem> FilterByQuestionnaires(IQueryable<QuestionnaireListViewItem> _, Guid userId, bool isAdmin)
        {
            var questionnaires = _.Where(x => x.IsDeleted == false);
            if (!isAdmin)
            {
                questionnaires = questionnaires.Where(questionnaire =>
                    questionnaire.CreatedBy == userId ||
                    questionnaire.SharedPersons.Any(person => person == userId) ||
                    questionnaire.IsPublic);
            }

            return questionnaires;
        }

        public QuestionnaireListView Load(QuestionnaireListInputModel input)
        {
            var count = questionnaireListViewItemStorage.Query(_ => FilterQuestionnaires(_, input).Count());

            var sortOrder = input.Order.IsNullOrEmpty() ? "LastEntryDate  Desc" : input.Order;

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
                })
                    .OrderUsingSortExpression(sortOrder)
                    .Skip((input.Page - 1)*input.PageSize)
                    .Take(input.PageSize)
                    .ToList());

            records.ForEach(x => x.Owner = x.CreatedBy == input.ViewerId ? "you" : x.CreatorName);
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