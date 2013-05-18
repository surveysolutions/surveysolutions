// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireHelper.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer
{
    using System;
    using System.Linq;

    using Main.Core.View;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Models;
    using WB.UI.Designer.Views.Questionnaire;
    using WB.UI.Shared.Web.Membership;

    /// <summary>
    ///     The questionnaire helper.
    /// </summary>
    public class QuestionnaireHelper : IQuestionnaireHelper
    {
        #region Fields

        /// <summary>
        /// The _user service.
        /// </summary>
        private readonly IMembershipUserService userService;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireHelper"/> class.
        /// </summary>
        /// <param name="userSevice">
        /// The user sevice.
        /// </param>
        public QuestionnaireHelper(IMembershipUserService userSevice)
        {
            this.userService = userSevice;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get public questionnaires.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        public IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            IViewRepository repository, 
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireListView model = this.GetQuestionnaireView(
                repository: repository, 
                userId: userId, 
                isOnlyOwnerItems: false, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(this.GetPublicQuestionnaire)
                        .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        /// <summary>
        /// The get items.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            IViewRepository repository, 
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireListView model = this.GetQuestionnaireView(
                repository: repository, 
                userId: userId, 
                isOnlyOwnerItems: true, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(this.GetQuestionnaire)
                        .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        /// <summary>
        /// The get questionnaires by user id.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        public IPagedList<QuestionnaireListViewModel> GetQuestionnairesByUserId(IViewRepository repository, Guid userId)
        {
            return this.GetQuestionnaires(repository: repository, userId: userId);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get public questionnaire.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <returns>
        /// The <see cref="QuestionnairePublicListViewModel"/>.
        /// </returns>
        private QuestionnairePublicListViewModel GetPublicQuestionnaire(QuestionnaireListViewItem x)
        {
            return new QuestionnairePublicListViewModel
                       {
                           Id = x.Id, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
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

        /// <summary>
        /// The get questionnaire.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <returns>
        /// The <see cref="QuestionnaireListViewModel"/>.
        /// </returns>
        private QuestionnaireListViewModel GetQuestionnaire(QuestionnaireListViewItem x)
        {
            return new QuestionnaireListViewModel
                       {
                           Id = x.Id, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           CanDelete = true, 
                           CanEdit = true, 
                           CanExport = true, 
                           CanSynchronize = this.userService.WebUser.IsAdmin
                       };
        }

        /// <summary>
        /// The get questionnaire view.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="isOnlyOwnerItems">
        /// The is only owner items.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="QuestionnaireListView"/>.
        /// </returns>
        private QuestionnaireListView GetQuestionnaireView(
            IViewRepository repository, 
            Guid userId, 
            bool isOnlyOwnerItems = true, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            return
                repository.Load<QuestionnaireListViewInputModel, QuestionnaireListView>(
                    input:
                        new QuestionnaireListViewInputModel
                            {
                                CreatedBy = userId, 
                                IsOnlyOwnerItems = isOnlyOwnerItems, 
                                IsAdminMode = this.userService.WebUser.IsAdmin, 
                                Page = pageIndex ?? 1, 
                                PageSize = GlobalHelper.GridPageItemsCount, 
                                Order = sortBy, 
                                Filter = filter
                            });
        }

        #endregion
    }

    /// <summary>
    /// The QuestionnaireHelper interface.
    /// </summary>
    public interface IQuestionnaireHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get public questionnaires.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            IViewRepository repository, 
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null);

        /// <summary>
        /// The get questionnaires.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="sortBy">
        /// The sort by.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            IViewRepository repository, 
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null);

        /// <summary>
        /// The get questionnaires by user id.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="IPagedList"/>.
        /// </returns>
        IPagedList<QuestionnaireListViewModel> GetQuestionnairesByUserId(IViewRepository repository, Guid userId);

        #endregion
    }
}