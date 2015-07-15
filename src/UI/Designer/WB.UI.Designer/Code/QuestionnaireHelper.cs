using System;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Membership;

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

        public IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            Guid viewerId, 
            bool isAdmin,
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireListView model = this.GetQuestionnaireView(
                viewerId: viewerId, 
                isPublic: true, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                isAdmin:isAdmin,
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(x => this.GetPublicQuestionnaire(x, viewerId, isAdmin))
                        .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            Guid viewerId,
            bool isAdmin,
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireListView model = this.GetQuestionnaireView(
                viewerId: viewerId, 
                isPublic: false, 
                isAdmin:isAdmin,
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            var result = model.Items.Select(x => this.GetQuestionnaire(x, viewerId, isAdmin)).ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
            return result;
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnairesByViewerId(Guid viewerId, bool isAdmin)
        {
            return this.GetQuestionnaires(viewerId: viewerId, isAdmin: isAdmin);
        }

        private QuestionnairePublicListViewModel GetPublicQuestionnaire(QuestionnaireListViewItem x, Guid viewerId, bool isAdmin)
        {
            var hasEditAccess = (x.CreatedBy == viewerId ||
                x.SharedPersons.Contains(viewerId) ||
                isAdmin) &&
                !x.IsDeleted;
            return new QuestionnairePublicListViewModel
                       {
                           Id = x.PublicId, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           CanDelete =
                               (x.CreatedBy == viewerId
                               || isAdmin) && !x.IsDeleted, 
                           CanExport = true,
                           CanOpen = !x.IsDeleted,
                           CanEdit = hasEditAccess,
                           CanSynchronize = isAdmin, 
                           CanExportToPdf = true,
                           CreatorName =
                               x.CreatedBy == null
                                   ? GlobalHelper.EmptyString
                                   : x.CreatorName
                       };
        }

        private QuestionnaireListViewModel GetQuestionnaire(QuestionnaireListViewItem x, Guid viewerId, bool isAdmin)
        {
            return new QuestionnaireListViewModel
                       {
                           Id = x.PublicId,
                           Owner = x.Owner,
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           IsPublic = x.IsPublic,
                           CanExportToPdf = true,
                           CanDelete = x.CreatedBy == viewerId && !x.IsDeleted,
                           CanOpen = (x.CreatedBy == viewerId ||
                                     x.SharedPersons.Contains(viewerId))
                                      && !x.IsDeleted,
                           CanExport = true,
                           CanSynchronize = isAdmin
                       };
        }

        private QuestionnaireListView GetQuestionnaireView(
            Guid viewerId, 
            bool isAdmin,
            bool isPublic = true, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            return
                this.viewFactory.Load(
                    input:
                        new QuestionnaireListInputModel
                            {
                                ViewerId = viewerId, 
                                IsPublic = isPublic,
                                IsAdminMode = isAdmin, 
                                Page = pageIndex ?? 1,
                                PageSize = GlobalHelper.GridPageItemsCount, 
                                Order = sortBy, 
                                Filter = filter
                            });
        }
    }
}