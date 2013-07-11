namespace WB.UI.Designer
{
    using System;
    using System.Linq;

    using Main.Core.View;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Models;
    using WB.UI.Designer.Views.Questionnaire;
    using WB.UI.Shared.Web.Membership;

    public class QuestionnaireHelper : IQuestionnaireHelper
    {
        private readonly IMembershipUserService userService;
        private readonly IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView> viewFactory;

        public QuestionnaireHelper(IMembershipUserService userSevice, IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView> viewFactory)
        {
            this.userService = userSevice;
            this.viewFactory = viewFactory;
        }

        public IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireListView model = this.GetQuestionnaireView(
                userId: userId, 
                isPublic: true, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(this.GetPublicQuestionnaire)
                        .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireListView model = this.GetQuestionnaireView(
                userId: userId, 
                isPublic: false, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(this.GetQuestionnaire)
                        .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnairesByUserId(Guid userId)
        {
            return this.GetQuestionnaires(userId: userId);
        }

        private QuestionnairePublicListViewModel GetPublicQuestionnaire(QuestionnaireListViewItem x)
        {
            return new QuestionnairePublicListViewModel
                       {
                           Id = x.Id, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           IsPublic = x.IsPublic,
                           CanDelete =
                               x.CreatedBy == this.userService.WebUser.UserId
                               || this.userService.WebUser.IsAdmin, 
                           CanExport = true, 
                           CanEdit = x.CreatedBy == this.userService.WebUser.UserId, 
                           CanSynchronize = this.userService.WebUser.IsAdmin, 
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
                           Id = x.Id, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           IsPublic = x.IsPublic,
                           CanDelete = true, 
                           CanEdit = true, 
                           CanExport = true, 
                           CanSynchronize = this.userService.WebUser.IsAdmin
                       };
        }

        private QuestionnaireListView GetQuestionnaireView(
            Guid userId, 
            bool isPublic = true, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            return
                this.viewFactory.Load(
                    input:
                        new QuestionnaireListViewInputModel
                            {
                                CreatedBy = userId, 
                                IsPublic = isPublic, 
                                IsAdminMode = this.userService.WebUser.IsAdmin, 
                                Page = pageIndex ?? 1, 
                                PageSize = GlobalHelper.GridPageItemsCount, 
                                Order = sortBy, 
                                Filter = filter
                            });
        }
    }

    public interface IQuestionnaireHelper
    {
        IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null);

        IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null);

        IPagedList<QuestionnaireListViewModel> GetQuestionnairesByUserId(Guid userId);
    }
}