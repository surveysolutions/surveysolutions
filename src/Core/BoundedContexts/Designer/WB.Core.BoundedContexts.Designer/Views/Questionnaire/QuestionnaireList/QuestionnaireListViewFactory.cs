using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    internal class QuestionnaireListViewFactory : IQuestionnaireListViewFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage;
        private readonly IPlainStorageAccessor<QuestionnaireListViewFolder> publicFoldersStorage;

        public QuestionnaireListViewFactory(IPlainStorageAccessor<QuestionnaireListViewItem> questionnaireListViewItemStorage,
            IPlainStorageAccessor<QuestionnaireListViewFolder> publicFoldersStorage)
        {
            this.questionnaireListViewItemStorage = questionnaireListViewItemStorage;
            this.publicFoldersStorage = publicFoldersStorage;
        }

        public IReadOnlyCollection<QuestionnaireListViewItem> GetUserQuestionnaires(
            Guid userId, bool isAdmin, int pageIndex = 1, int pageSize = 128)
        {
            return questionnaireListViewItemStorage.Query(queryable
                => FilterByQuestionnaires(queryable, userId, isAdmin)
                    .OrderBy(x => x.Title)
                    .Skip((pageIndex - 1)*pageSize)
                    .Take(pageSize)
                    .ToReadOnlyCollection());
        }

        private static IQueryable<QuestionnaireListViewItem> FilterByQuestionnaires(
            IQueryable<QuestionnaireListViewItem> queryable, Guid userId, bool isAdmin)
        {
            var notDeletedQuestionnaires = queryable.Where(x => x.IsDeleted == false);

            return isAdmin
                ? notDeletedQuestionnaires
                : notDeletedQuestionnaires.Where(questionnaire =>
                    questionnaire.CreatedBy == userId ||
                    questionnaire.SharedPersons.Any(person => person.UserId == userId) ||
                    questionnaire.IsPublic);
        }

        public QuestionnaireListView Load(QuestionnaireListInputModel input)
        {
            var count = questionnaireListViewItemStorage.Query(_ => FilterQuestionnaires(_, input, isSupportFolders: false).Count());

            var sortOrder = input.Order.IsNullOrEmpty() ? "LastEntryDate  Desc" : input.Order;

            var records = questionnaireListViewItemStorage.Query(_ =>
                FilterQuestionnaires(_, input, isSupportFolders: false).Select(x => new QuestionnaireListViewItem()
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
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList());

            return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                items: records,
                order: input.Order);
        }

        public QuestionnaireListView LoadFoldersAndQuestionnaires(QuestionnaireListInputModel input)
        {
            List<QuestionnaireListViewFolder> folders = Enumerable.Empty<QuestionnaireListViewFolder>().ToList();
            List<QuestionnaireListViewItem> questionnaires = Enumerable.Empty<QuestionnaireListViewItem>().ToList();
            int foldersCount = 0, questionnairesCount = 0;
            var isSupportFolders = input.IsPublic;

            if (isSupportFolders)
            {
                var sortOrder = ConvertToFolderSortOrder(input);

                foldersCount = publicFoldersStorage.Query(_ => FilterFolders(_, input).Count());
                folders = publicFoldersStorage.Query(_ => FilterFolders(_, input)
                    .OrderUsingSortExpression(sortOrder)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList());
            }

            questionnairesCount = questionnaireListViewItemStorage.Query(_ => FilterQuestionnaires(_, input, isSupportFolders: isSupportFolders).Count());
            var count = foldersCount + questionnairesCount;

            if (folders.Count < input.PageSize)
            {
                var questionnairesSkipCount = (input.Page - 1) * input.PageSize - foldersCount;
                questionnairesSkipCount = questionnairesSkipCount < 0 ? 0 : questionnairesSkipCount;
                var questionnairesTakeCount = input.PageSize - folders.Count;

                var sortOrder = input.Order.IsNullOrEmpty() ? "LastEntryDate  Desc" : input.Order;

                questionnaires = questionnaireListViewItemStorage.Query(_ =>
                    FilterQuestionnaires(_, input, isSupportFolders: isSupportFolders).Select(x => new QuestionnaireListViewItem()
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
                            FolderId = x.FolderId,
                            Folder = isSupportFolders ? x.Folder : null
                        })
                        .OrderUsingSortExpression(sortOrder)
                        .Skip(questionnairesSkipCount)
                        .Take(questionnairesTakeCount)
                        .ToList());
            }

            var items = folders.Concat(questionnaires.Cast<IQuestionnaireListItem>()).ToList();
            return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                items: items, order: input.Order);
        }

        public IEnumerable<QuestionnaireListFolderLocation> LoadFoldersLocation(IEnumerable<QuestionnaireListViewFolder> folders)
        {
            HashSet<Guid> foldersIds = new HashSet<Guid>();
            folders.ForEach(f =>
            {
                var foldersFromPath = f.Path.Split('/');
                foreach (var pathFolder in foldersFromPath)
                {
                    if (!string.IsNullOrEmpty(pathFolder))
                    {
                        foldersIds.Add(Guid.Parse(pathFolder));
                    }
                }
            });

            var titles = publicFoldersStorage.Query(_ => 
                _.Where(f => foldersIds.Contains(f.PublicId))
                    .Select(f => new{ PublicId = f.PublicId, Title = f.Title })
            ).ToDictionary(k => k.PublicId, v => v.Title);

            return folders.Select(f => new QuestionnaireListFolderLocation()
            {
                PublicId = f.PublicId,
                Location = string.Join(" / ", f.Path.Split('/').Where(fp => !string.IsNullOrEmpty(fp)).Select(fp => titles[Guid.Parse(fp)]))
            }).ToList();
        }

        private static string ConvertToFolderSortOrder(QuestionnaireListInputModel input)
        {
            switch (input.Order)
            {
                case "CreationDate  Desc":
                case "LastEntryDate  Desc":
                    return nameof(QuestionnaireListViewFolder.CreateDate) + "  Desc";
                case "CreationDate":
                case "LastEntryDate":
                    return nameof(QuestionnaireListViewFolder.CreateDate);
                case "Title  Desc":
                    return nameof(QuestionnaireListViewFolder.Title) + "  Desc";
                default:
                    return nameof(QuestionnaireListViewFolder.Title);
            }
        }

        private IQueryable<QuestionnaireListViewItem> FilterQuestionnaires(IQueryable<QuestionnaireListViewItem> _,
            QuestionnaireListInputModel input, bool isSupportFolders)
        {
            var result = _.Where(x => x.IsDeleted == false);

            if (isSupportFolders)
            {
                if (!string.IsNullOrEmpty(input.SearchFor))
                {
                    if (input.FolderId.HasValue)
                    {
                        var folderId = input.FolderId.Value.ToString();
                        result = result.Where(x => x.Folder.Path.Contains(folderId));
                    }
                }
                else
                {
                    result = result.Where(x => x.FolderId == input.FolderId);
                }
            }

            if (!string.IsNullOrEmpty(input.SearchFor))
            {
                var filterLowerCase = input.SearchFor.Trim().ToLower();
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
                                x.SharedPersons.Any(person => person.UserId == input.ViewerId));
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
                                x.SharedPersons.Any(person => person.UserId == input.ViewerId));
            }
            return result;
        }

        private IQueryable<QuestionnaireListViewFolder> FilterFolders(IQueryable<QuestionnaireListViewFolder> _,
            QuestionnaireListInputModel input)
        {
            var result = _.Where(x => input.IsPublic == true); // support only public folders

            if (!string.IsNullOrEmpty(input.SearchFor))
            {
                var filterLowerCase = input.SearchFor.Trim().ToLower();
                result = result.Where(x => x.Title.ToLower().Contains(filterLowerCase));

                if (input.FolderId.HasValue)
                {
                    var folderId = input.FolderId.Value.ToString();
                    result = result.Where(f => f.Path.Contains(folderId));
                }
            }
            else 
            {
                result = result.Where(x => input.FolderId == x.Parent);
            }

            return result;
        }
    }
}