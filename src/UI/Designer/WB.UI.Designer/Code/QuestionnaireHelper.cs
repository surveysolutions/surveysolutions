using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code
{
    public class QuestionnaireHelper : IQuestionnaireHelper
    {
        private readonly IQuestionnaireListViewFactory viewFactory;

        public QuestionnaireHelper(
            IQuestionnaireListViewFactory viewFactory)
        {
            this.viewFactory = viewFactory;
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(Guid viewerId, bool isAdmin, QuestionnairesType type, Guid? folderId,
            int? pageIndex = null, string sortBy = null, int? sortOrder = null, string searchFor = null)
        {
            QuestionnaireListView model = this.viewFactory.LoadFoldersAndQuestionnaires(new QuestionnaireListInputModel
            {
                ViewerId = viewerId,
                Type = type,
                IsAdminMode = isAdmin,
                Page = pageIndex ?? 1,
                PageSize = GlobalHelper.GridPageItemsCount,
                Order = sortBy,
                SearchFor = searchFor,
                FolderId = folderId
            });

            var showPublic = type == QuestionnairesType.Public;
            var locations = showPublic ? GetLocations(model.Items) : new Dictionary<Guid, string>();

            return model.Items.Select(x =>
                {
                    if (x is QuestionnaireListViewItem item)
                    {
                        var questLocation = item.Folder != null && locations.ContainsKey(item.Folder.PublicId) 
                                            ? locations[item.Folder.PublicId] 
                                            : null;
                        return this.GetQuestionnaire(item, viewerId, isAdmin, showPublic, questLocation);
                    }

                    var folderLocation = locations.ContainsKey(x.PublicId) ? locations[x.PublicId] : null;
                    return this.GetFolder((QuestionnaireListViewFolder)x, showPublic, folderLocation);
                })
                .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        private Dictionary<Guid, string> GetLocations(IEnumerable<IQuestionnaireListItem> modelItems)
        {
            HashSet<QuestionnaireListViewFolder> folders = new HashSet<QuestionnaireListViewFolder>();

            foreach (var modelItem in modelItems)
            {
                if (modelItem is QuestionnaireListViewItem item && item.Folder != null)
                    folders.Add(item.Folder);
                if (modelItem is QuestionnaireListViewFolder folder)
                    folders.Add(folder);
            }

            var locations = viewFactory.LoadFoldersLocation(folders);

            return locations.ToDictionary(k => k.PublicId, v => v.Location);
        }

        public IPagedList<QuestionnaireListViewModel> GetMyQuestionnairesByViewerId(Guid viewerId, bool isAdmin, Guid? folderId = null)
            => GetQuestionnaires(viewerId: viewerId, isAdmin: isAdmin, type: QuestionnairesType.My, folderId: folderId);

        public IPagedList<QuestionnaireListViewModel> GetSharedQuestionnairesByViewerId(Guid viewerId, bool isAdmin, Guid? folderId)
            => this.GetQuestionnaires(viewerId: viewerId, isAdmin: isAdmin, type: QuestionnairesType.Shared, folderId: folderId);

        private QuestionnaireListViewModel GetQuestionnaire(QuestionnaireListViewItem x, Guid viewerId, bool isAdmin, bool showPublic, string location)
            => new QuestionnaireListViewModel
            {
                Id = x.PublicId.FormatGuid(),
                CreationDate = x.CreationDate,
                LastEntryDate = x.LastEntryDate,
                Title = x.Title,
                IsDeleted = x.IsDeleted,
                IsPublic = showPublic,
                CanDelete = x.CreatedBy == viewerId && !x.IsDeleted,
                CanExport = true,
                CanCopy = true,
                CanAssignFolder = showPublic && isAdmin,
                CanOpen = (showPublic || x.CreatedBy == viewerId || x.SharedPersons.Any(s => s.UserId == viewerId)) && !x.IsDeleted,
                CanSynchronize = isAdmin,
                CanExportToPdf = true,
                CanExportToHtml = true,
                Location = location != null
                           ? x.Title + "\r\n" + @QuestionnaireController.Location + QuestionnaireController.PublicQuestionnaires + " / " + location
                           : null,
                Owner = x.CreatedBy == null
                    ? GlobalHelper.EmptyString
                    : (x.CreatedBy == viewerId ? QuestionnaireController.You : x.CreatorName)
            };

        private QuestionnaireListViewModel GetFolder(QuestionnaireListViewFolder x, bool showPublic, string location)
            => new QuestionnaireListViewModel
            {
                Id = x.PublicId.FormatGuid(),
                IsFolder = true,
                CreationDate = x.CreateDate,
                LastEntryDate = x.CreateDate,
                Title = x.Title,
                IsPublic = showPublic,
                CanDelete = false,
                CanCopy = false,
                CanExport = false,
                CanOpen = showPublic,
                CanEdit = false,
                CanSynchronize = false,
                CanExportToPdf = false,
                CanExportToHtml = false,
                Location = location != null ? QuestionnaireController.Location + QuestionnaireController.PublicQuestionnaires + " / " + location : null,
                Owner = GlobalHelper.EmptyString
            };
    }
}