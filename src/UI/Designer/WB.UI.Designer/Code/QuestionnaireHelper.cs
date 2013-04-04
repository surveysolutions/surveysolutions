// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.UI.Designer.Code;

namespace WB.UI.Designer
{
    using System;
    using System.Linq;
    using System.Web.Security;

    using Main.Core.View;
    using Main.Core.View.Questionnaire;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Models;

    /// <summary>
    ///     The questionnaire helper.
    /// </summary>
    public class QuestionnaireHelper
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
        public static IPagedList<QuestionnairePublicListViewModel> GetPublicQuestionnaires(
            IViewRepository repository, 
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireBrowseView model = GetQuestionnaireView(
                repository: repository, 
                userId: userId, 
                isOnlyOwnerItems: false, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(GetPublicQuestionnaire)
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
        public static IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            IViewRepository repository, 
            Guid userId, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireBrowseView model = GetQuestionnaireView(
                repository: repository, 
                userId: userId, 
                isOnlyOwnerItems: true, 
                pageIndex: pageIndex, 
                sortBy: sortBy, 
                sortOrder: sortOrder, 
                filter: filter);

            return model.Items.Select(GetQuestionnaire)
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
        public static IPagedList<QuestionnaireListViewModel> GetQuestionnairesByUserId(
            IViewRepository repository, Guid userId)
        {
            return GetQuestionnaires(repository: repository, userId: userId);
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
        private static QuestionnairePublicListViewModel GetPublicQuestionnaire(QuestionnaireBrowseItem x)
        {
            var createdBy = Membership.GetUser(x.CreatedBy, false);

            return new QuestionnairePublicListViewModel
                       {
                           Id = x.Id, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           CanDelete = UserHelper.IsAdmin, 
                           CanEdit = false, 
                           CreatedBy =
                               createdBy == null ? GlobalHelper.EmptyString : createdBy.UserName
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
        private static QuestionnaireListViewModel GetQuestionnaire(QuestionnaireBrowseItem x)
        {
            return new QuestionnaireListViewModel
                       {
                           Id = x.Id, 
                           CreationDate = x.CreationDate, 
                           LastEntryDate = x.LastEntryDate, 
                           Title = x.Title, 
                           IsDeleted = x.IsDeleted, 
                           CanDelete = true, 
                           CanEdit = true
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
        /// The <see cref="QuestionnaireBrowseView"/>.
        /// </returns>
        private static QuestionnaireBrowseView GetQuestionnaireView(
            IViewRepository repository, 
            Guid userId, 
            bool isOnlyOwnerItems = true, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            return
                repository.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(
                    input:
                        new QuestionnaireBrowseInputModel
                            {
                                CreatedBy = userId, 
                                IsOnlyOwnerItems = isOnlyOwnerItems, 
                                IsAdminMode = UserHelper.IsAdmin, 
                                Page = pageIndex ?? 1, 
                                PageSize = GlobalHelper.GridPageItemsCount, 
                                Order = sortBy, 
                                Filter = filter
                            });
        }

        #endregion
    }
}