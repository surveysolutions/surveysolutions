using System;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services;
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

        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(Guid viewerId, bool isAdmin, bool showPublic, Guid? folderId,
            int? pageIndex = null, string sortBy = null, int? sortOrder = null, string searchFor = null)
        {
            QuestionnaireListView model = this.viewFactory.LoadFoldersAndQuestionnaires(new QuestionnaireListInputModel
            {
                ViewerId = viewerId,
                IsPublic = showPublic,
                IsAdminMode = isAdmin,
                Page = pageIndex ?? 1,
                PageSize = GlobalHelper.GridPageItemsCount,
                Order = sortBy,
                SearchFor = searchFor,
                FolderId = folderId
            });

            return model.Items.Select(x =>
                {
                    if (x is QuestionnaireListViewItem item)
                        return this.GetQuestionnaire(item, viewerId, isAdmin, showPublic);

                    return this.GetFolder((QuestionnaireListViewFolder)x, showPublic);
                })
                .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnairesByViewerId(Guid viewerId, bool isAdmin, Guid? folderId = null) 
            => this.GetQuestionnaires(viewerId: viewerId, isAdmin: isAdmin, showPublic: false, folderId: folderId);

        private QuestionnaireListViewModel GetQuestionnaire(QuestionnaireListViewItem x, Guid viewerId, bool isAdmin, bool showPublic)
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
                Owner = x.CreatedBy == null
                    ? GlobalHelper.EmptyString
                    : (x.CreatedBy == viewerId ? QuestionnaireController.You : x.CreatorName)
            };

        private QuestionnaireListViewModel GetFolder(QuestionnaireListViewFolder x, bool showPublic)
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
                Owner = GlobalHelper.EmptyString
            };
    }
}