// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.DenormalizerStorage;

namespace Main.Core.View.Questionnaire
{
    /// <summary>
    /// The questionnaire browse view factory.
    /// </summary>
    public class QuestionnaireBrowseViewFactory : IViewFactory<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document group session.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireDocument> documentGroupSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public QuestionnaireBrowseViewFactory(IDenormalizerStorage<QuestionnaireDocument> documentGroupSession)
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
        /// The RavenQuestionnaire.Core.Views.Questionnaire.QuestionnaireBrowseView.
        /// </returns>
        public QuestionnaireBrowseView Load(QuestionnaireBrowseInputModel input)
        {
            IQueryable<QuestionnaireDocument> query = this.documentGroupSession.Query();

            // Adjust the model appropriately
            int count = query.Count();
            if (count == 0)
            {
                return new QuestionnaireBrowseView(
                    input.Page, input.PageSize, count, new QuestionnaireBrowseItem[0], string.Empty);
            }

            // Perform the paged query

            /*   if (input.Orders.Count > 0)
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(input.Orders[0].Field)
                            : query.OrderByDescending(input.Orders[0].Field);

            }
            if (input.Orders.Count > 1)
                foreach (var order in input.Orders.Skip(1))
                {
                    query = order.Direction == OrderDirection.Asc
                                ? query.ThenBy(order.Field)
                                : query.ThenByDescending(order.Field);
                }*/
            List<QuestionnaireDocument> page = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();

            // And enact this query
            QuestionnaireBrowseItem[] items =
                page.Select(x => new QuestionnaireBrowseItem(x.PublicKey, x.Title, DateTime.Now, DateTime.Now)).
                    ToArray();

            return new QuestionnaireBrowseView(input.Page, input.PageSize, count, items, input.Order);
        }

        #endregion
    }
}