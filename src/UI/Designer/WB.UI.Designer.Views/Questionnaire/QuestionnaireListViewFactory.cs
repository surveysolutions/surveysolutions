// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;
using Main.DenormalizerStorage;
using System;
using System.Linq;
using Main.Core.Utility;

namespace WB.UI.Designer.Views.Questionnaire
{
    /// <summary>
    /// The questionnaire browse view factory.
    /// </summary>
    public class QuestionnaireListViewFactory : IViewFactory<QuestionnaireListViewInputModel, QuestionnaireListView>
    {
        #region Fields

        /// <summary>
        /// The document group session.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireListViewItem> documentGroupSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireListViewFactory"/> class.
        /// </summary>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public QuestionnaireListViewFactory(IDenormalizerStorage<QuestionnaireListViewItem> documentGroupSession)
        {
            this.documentGroupSession = documentGroupSession;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The QuestionnaireBrowseView.
        /// </returns>
        public QuestionnaireListView Load(QuestionnaireListViewInputModel input)
        {
            // Adjust the model appropriately
            int count = this.documentGroupSession.Count();
            if (count == 0)
            {
                return new QuestionnaireListView(
                    input.Page, input.PageSize, count, new QuestionnaireListViewItem[0], string.Empty);
            }

            IQueryable<QuestionnaireListViewItem> query = this.documentGroupSession.Query();

            Func<QuestionnaireListViewItem, bool> q = (ret) => true;

            if (input.IsAdminMode.HasValue)
            {
                q =
                    x =>
                    (!input.IsOnlyOwnerItems || x.CreatedBy == input.CreatedBy)
                    && (input.IsAdminMode.Value || !x.IsDeleted)
                    && (string.IsNullOrEmpty(input.Filter) || x.Title.ContainsIgnoreCaseSensitive(input.Filter));
            }

            var queryResult = query.Where(q).AsQueryable().OrderUsingSortExpression(input.Order);

            var questionnaireItems = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();


            return new QuestionnaireListView(input.Page, input.PageSize, queryResult.Count(), questionnaireItems, input.Order);
        }

        #endregion
    }
}