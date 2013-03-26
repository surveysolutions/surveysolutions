// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;
using System;
using System.Linq;

namespace Main.Core.View.Questionnaire
{
    using Main.Core.Utility;

    /// <summary>
    /// The questionnaire browse view factory.
    /// </summary>
    public class QuestionnaireBrowseViewFactory : IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document group session.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireBrowseItem> documentGroupSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public QuestionnaireBrowseViewFactory(IDenormalizerStorage<QuestionnaireBrowseItem> documentGroupSession)
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
        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
            int count = this.documentGroupSession.Count();
            if (count == 0)
            {
                return new QuestionnaireBrowseView(
                    input.Page, input.PageSize, count, new QuestionnaireBrowseItem[0], string.Empty);
            }

            IQueryable<QuestionnaireBrowseItem> query = this.documentGroupSession.Query();

            Func<QuestionnaireBrowseItem, bool> q = (ret) => true;

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


            return new QuestionnaireBrowseView(input.Page, input.PageSize, queryResult.Count(), questionnaireItems, input.Order);
        }

        #endregion
    }
}