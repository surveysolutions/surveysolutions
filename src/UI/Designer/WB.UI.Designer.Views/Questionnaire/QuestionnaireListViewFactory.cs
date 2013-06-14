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

using WB.Core.Infrastructure;

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
        private readonly IQueryableDenormalizerStorage<QuestionnaireListViewItem> documentGroupSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireListViewFactory"/> class.
        /// </summary>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public QuestionnaireListViewFactory(IQueryableDenormalizerStorage<QuestionnaireListViewItem> documentGroupSession)
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
            Func<QuestionnaireListViewItem, bool> q =
                (x) =>
                string.IsNullOrEmpty(input.Filter)
                || (x.Title.ContainsIgnoreCaseSensitive(input.Filter)
                    || x.CreatorName.ContainsIgnoreCaseSensitive(input.Filter));
             

            if (input.IsAdminMode)
            {
                q = q.AndAlso(x => (input.IsPublic || (x.CreatedBy == input.CreatedBy)));
            }
            else
            {
                q =
                    q.AndAlso(
                        x =>
                        !x.IsDeleted
                        && (((x.CreatedBy == input.CreatedBy) && !input.IsPublic) || (input.IsPublic && x.IsPublic)));
            }


            return this.documentGroupSession.Query(queryable =>
            {
                var queryResult = queryable.Where(q).AsQueryable().OrderUsingSortExpression(input.Order);

                var questionnaireItems = queryResult.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();


                return new QuestionnaireListView(input.Page, input.PageSize, queryResult.Count(), questionnaireItems, input.Order);
            });
        }

        #endregion
    }
}