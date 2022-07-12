using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireListViewFactory : IQuestionnaireListViewFactory
    {
        private readonly DesignerDbContext dbContext;

        public QuestionnaireListViewFactory(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IReadOnlyCollection<QuestionnaireListViewItem> GetUserQuestionnaires(
            Guid userId, bool isAdmin, int pageIndex = 1, int pageSize = 128)
        {
            return FilterByQuestionnaires(this.dbContext.Questionnaires.Include(x => x.SharedPersons).AsQueryable(), userId, isAdmin)
                .OrderBy(x => x.Title)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToReadOnlyCollection();
        }

        private static IQueryable<QuestionnaireListViewItem> FilterByQuestionnaires(
            IQueryable<QuestionnaireListViewItem> queryable, Guid userId, bool isAdmin)
        {
            var notDeletedQuestionnaires = queryable.Where(x => x.IsDeleted == false);

            return isAdmin
                ? notDeletedQuestionnaires
                : notDeletedQuestionnaires.Where(questionnaire =>
                    questionnaire.OwnerId == userId ||
                    questionnaire.SharedPersons.Any(person => person.UserId == userId) ||
                    questionnaire.IsPublic);
        }

        public QuestionnaireListView Load(QuestionnaireListInputModel input)
        {
            var count = FilterQuestionnaires(this.dbContext.Questionnaires.AsQueryable(), input, isSupportFolders: false).Count();

            var sortOrder = input.Order.IsNullOrEmpty() ? "LastEntryDate  Desc" : input.Order;

            var records = 
                FilterQuestionnaires(this.dbContext.Questionnaires.AsQueryable(), input, isSupportFolders: false)
                    .Select(x => new QuestionnaireListViewItem()
                    {
                        CreationDate = x.CreationDate,
                        CreatedBy = x.CreatedBy,
                        CreatorName = x.CreatorName,
                        OwnerId = x.OwnerId,
                        Owner = x.Owner,
                        IsDeleted = x.IsDeleted,
                        IsPublic = x.IsPublic,
                        LastEntryDate = x.LastEntryDate,
                        PublicId = x.PublicId,
                        QuestionnaireId = x.QuestionnaireId,
                        Title = x.Title
                    })
                    .OrderUsingSortExpression(sortOrder)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();

            return new QuestionnaireListView(page: input.Page, pageSize: input.PageSize, totalCount: count,
                items: records,
                order: input.Order);
        }

        public QuestionnaireListView LoadFoldersAndQuestionnaires(QuestionnaireListInputModel input)
        {
            List<QuestionnaireListViewFolder> folders = new List<QuestionnaireListViewFolder>();
            List<QuestionnaireListViewItem> questionnaires = new List<QuestionnaireListViewItem>(); 
            int foldersCount = 0, questionnairesCount = 0;
            var isSupportFolders = input.Type == QuestionnairesType.Public;

            if (isSupportFolders)
            {
                var sortOrder = ConvertToFolderSortOrder(input);

                foldersCount = FilterFolders(this.dbContext.QuestionnaireFolders.AsQueryable(), input).Count();
                folders = FilterFolders(this.dbContext.QuestionnaireFolders.AsQueryable(), input)
                    .OrderUsingSortExpression(sortOrder)
                    .Skip((input.Page - 1) * input.PageSize)
                    .Take(input.PageSize)
                    .ToList();
            }

            questionnairesCount = FilterQuestionnaires(this.dbContext.Questionnaires, input, isSupportFolders: isSupportFolders).Count();
            var count = foldersCount + questionnairesCount;

            if (folders.Count < input.PageSize)
            {
                var questionnairesSkipCount = (input.Page - 1) * input.PageSize - foldersCount;
                questionnairesSkipCount = questionnairesSkipCount < 0 ? 0 : questionnairesSkipCount;
                var questionnairesTakeCount = input.PageSize - folders.Count;

                var sortOrder = input.Order.IsNullOrEmpty() ? $"{nameof(QuestionnaireListViewItem.LastEntryDate)}  Desc" : input.Order;

                var questionnaireListViewItems = FilterQuestionnaires(this.dbContext.Questionnaires, input, isSupportFolders: isSupportFolders)
                    .OrderUsingSortExpression(sortOrder)
                    .Skip(questionnairesSkipCount)
                    .Take(questionnairesTakeCount)
                    .ToList();
                questionnaires = 
                    questionnaireListViewItems
                        .Select(x => new QuestionnaireListViewItem
                        {
                            OwnerId = x.OwnerId,
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
                            Folder = isSupportFolders ? x.Folder : null,
                            SharedPersons = x.SharedPersons
                        }).ToList();
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

            var titles = this.dbContext.QuestionnaireFolders.Where(f => foldersIds.Contains(f.PublicId))
                    .Select(f => new{ PublicId = f.PublicId, Title = f.Title })
                    .ToDictionary(k => k.PublicId, v => v.Title);

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

        private IQueryable<QuestionnaireListViewItem> FilterQuestionnaires(
            IQueryable<QuestionnaireListViewItem> _,
            QuestionnaireListInputModel input, bool isSupportFolders)
        {
            var result = _.Include(x => x.SharedPersons).Where(x => x.IsDeleted == false);

            if (isSupportFolders)
            {
                if (!string.IsNullOrEmpty(input.SearchFor))
                {
                    if (input.FolderId.HasValue)
                    {
                        var folderId = input.FolderId.Value.ToString();
                        result = result.Where(x => x.Folder!= null && x.Folder.Path.Contains(folderId));
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
                        x => x.Title.ToLower().Contains(filterLowerCase) || (x.CreatorName ?? "").ToLower().Contains(filterLowerCase));
            }

            result = result.Where(x =>
                   input.Type.HasFlag(QuestionnairesType.My) 
                        && x.OwnerId == input.ViewerId || 
                   
                   input.Type.HasFlag(QuestionnairesType.Shared) 
                        && x.OwnerId != input.ViewerId 
                        && x.SharedPersons.Any(person => person.UserId == input.ViewerId) ||
                   
                   input.Type.HasFlag(QuestionnairesType.Public) 
                        && (input.IsAdminMode || x.IsPublic)
            );
            
            return result;
        }

        private IQueryable<QuestionnaireListViewFolder> FilterFolders(IQueryable<QuestionnaireListViewFolder> _,
            QuestionnaireListInputModel input)
        {
            if (string.IsNullOrEmpty(input.SearchFor))
                return _.Where(x => input.FolderId == x.Parent);

            var filterLowerCase = input.SearchFor.Trim().ToLower();
            _ = _.Where(x => x.Title.ToLower().Contains(filterLowerCase));

            if (input.FolderId.HasValue)
            {
                var folderId = input.FolderId.Value.ToString();
                _ = _.Where(f => f.Path.Contains(folderId));
            }

            return _;
        }
    }
}
