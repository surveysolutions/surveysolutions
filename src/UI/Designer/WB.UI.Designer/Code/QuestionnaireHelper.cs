// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The questionnaire helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer
{
    using System;
    using System.Linq;

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
        /// The get items.
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
        /// The <see cref="IPagedList"/>.
        /// </returns>
        public static IPagedList<QuestionnaireListViewModel> GetQuestionnaires(
            IViewRepository repository, 
            Guid userId, 
            bool isOnlyOwnerItems = true, 
            int? pageIndex = null, 
            string sortBy = null, 
            int? sortOrder = null, 
            string filter = null)
        {
            QuestionnaireBrowseView model =
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
            IPagedList<QuestionnaireListViewModel> retVal =
                model.Items.Select(
                    x =>
                    new QuestionnaireListViewModel
                        {
                            Id = x.Id, 
                            CreationDate = x.CreationDate, 
                            LastEntryDate = x.LastEntryDate, 
                            Title = x.Title, 
                            IsDeleted = x.IsDeleted, 
                            CanDelete = isOnlyOwnerItems || UserHelper.IsAdmin, 
                            CanEdit = isOnlyOwnerItems
                        })
                     .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);

            return retVal;
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
    }
}