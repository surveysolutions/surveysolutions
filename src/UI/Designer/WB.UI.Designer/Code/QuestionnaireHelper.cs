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
        private readonly IMembershipUserService userService;
        private readonly IQuestionnaireListViewFactory viewFactory;

        public QuestionnaireHelper(IMembershipUserService userSevice,
            IQuestionnaireListViewFactory viewFactory)
        {
            this.userService = userSevice;
            this.viewFactory = viewFactory;
        }

        public IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            Guid viewerId, 
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
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(this.GetPublicQuestionnaire)
                        .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            Guid viewerId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireListView model = this.GetQuestionnaireView(
                viewerId: viewerId, 
                isPublic: false, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            var result = model.Items.Select(this.GetQuestionnaire)
                                    .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
            return result;
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnairesByViewerId(Guid viewerId)
        {
            return this.GetQuestionnaires(viewerId: viewerId);
        }

        private QuestionnairePublicListViewModel GetPublicQuestionnaire(QuestionnaireListViewItem x)
        {
            var hasAccess = (x.CreatedBy == this.userService.WebUser.UserId ||
                x.SharedPersons.Contains(this.userService.WebUser.UserId) ||
                this.userService.WebUser.IsAdmin) &&
                !x.IsDeleted;
            return new QuestionnairePublicListViewModel
                       {
                           Id = x.PublicId, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           CanDelete =
                               (x.CreatedBy == this.userService.WebUser.UserId
                               || this.userService.WebUser.IsAdmin) && !x.IsDeleted, 
                           CanExport = true, 
                           CanOpen = hasAccess,
                           CanEdit = hasAccess,
                           CanSynchronize = this.userService.WebUser.IsAdmin, 
                           CanExportToPdf = true,
                           CreatorName =
                               x.CreatedBy == null
                                   ? GlobalHelper.EmptyString
                                   : x.CreatorName
                       };
        }

        private QuestionnaireListViewModel GetQuestionnaire(QuestionnaireListViewItem x)
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
                           CanDelete = x.CreatedBy == this.userService.WebUser.UserId && !x.IsDeleted,
                           CanOpen = (x.CreatedBy == this.userService.WebUser.UserId ||
                                     x.SharedPersons.Contains(this.userService.WebUser.UserId))
                                      && !x.IsDeleted,
                           CanExport = true, 
                           CanSynchronize = this.userService.WebUser.IsAdmin
                       };
        }

        private QuestionnaireListView GetQuestionnaireView(
            Guid viewerId, 
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
                                IsAdminMode = this.userService.WebUser.IsAdmin, 
                                Page = pageIndex ?? 1,
                                PageSize = GlobalHelper.GridPageItemsCount, 
                                Order = sortBy, 
                                Filter = filter
                            });
        }
    }
}