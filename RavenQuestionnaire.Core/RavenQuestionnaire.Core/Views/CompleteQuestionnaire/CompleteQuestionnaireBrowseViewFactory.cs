// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Denormalizers;

    /// <summary>
    /// The complete questionnaire browse view factory.
    /// </summary>
    public class CompleteQuestionnaireBrowseViewFactory :
        IViewFactory<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CompleteQuestionnaireBrowseViewFactory(
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession)
        {
            this.documentSession = documentSession;
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
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.CompleteQuestionnaireBrowseView.
        /// </returns>
        public CompleteQuestionnaireBrowseView Load(CompleteQuestionnaireBrowseInputModel input)
        {
            // Adjust the model appropriately
            int count = this.documentSession.Query().Count();
            if (count == 0)
            {
                return new CompleteQuestionnaireBrowseView(
                    input.Page, input.PageSize, count, new CompleteQuestionnaireBrowseItem[0], input.Order);
            }

            IQueryable<CompleteQuestionnaireBrowseItem> query;

            if (!string.IsNullOrEmpty(input.ResponsibleId))
            {
                // filter result by responsible
                query = this.documentSession.Query().Where(x => x.Responsible.Id == input.ResponsibleId);
            }
            else
            {
                query = this.documentSession.Query();
            }

            /* if (input.Orders.Count > 0)
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
                }
            */
            CompleteQuestionnaireBrowseItem[] page =
                query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();

            IEnumerable<CompleteQuestionnaireBrowseItem> items =
                page.Select(
                    x =>
                    new CompleteQuestionnaireBrowseItem(
                        x.CompleteQuestionnaireId, 
                        x.QuestionnaireTitle, 
                        x.TemplateId, 
                        x.CreationDate, 
                        x.LastEntryDate, 
                        x.Status, 
                        0, 
                        0, 
                        x.Responsible));

            return new CompleteQuestionnaireBrowseView(input.Page, input.PageSize, count, items, input.Order);
        }

        #endregion
    }
}