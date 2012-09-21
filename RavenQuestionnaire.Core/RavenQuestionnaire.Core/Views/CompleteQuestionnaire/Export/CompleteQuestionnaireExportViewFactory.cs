// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.View;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Utility;

    using Raven.Client;

    /// <summary>
    /// The complete questionnaire export view factory.
    /// </summary>
    public class CompleteQuestionnaireExportViewFactory :
        IViewFactory<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IDocumentSession documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CompleteQuestionnaireExportViewFactory(IDocumentSession documentSession)
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
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export.CompleteQuestionnaireExportView.
        /// </returns>
        public CompleteQuestionnaireExportView Load(CompleteQuestionnaireExportInputModel input)
        {
            // Adjust the model appropriately
            int count = this.documentSession.Query<CompleteQuestionnaireDocument>().Count();
            if (count == 0)
            {
                return new CompleteQuestionnaireExportView(
                    input.Page, input.PageSize, count, new CompleteQuestionnaireExportItem[0], input.Order);
            }

            IOrderedQueryable<CompleteQuestionnaireDocument> query;

            if (Guid.Empty != input.QuestionnaryId)
            {
                query =
                    (IOrderedQueryable<CompleteQuestionnaireDocument>)
                    this.documentSession.Query<CompleteQuestionnaireDocument>().Where(
                        x => x.TemplateId == input.QuestionnaryId);
            }
            else
            {
                return new CompleteQuestionnaireExportView(
                    input.Page, input.PageSize, count, new CompleteQuestionnaireExportItem[0], input.Order);
            }

            if (input.Orders.Count > 0)
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(input.Orders[0].Field)
                            : query.OrderByDescending(input.Orders[0].Field);
            }

            if (input.Orders.Count > 1)
            {
                foreach (OrderRequestItem order in input.Orders.Skip(1))
                {
                    query = order.Direction == OrderDirection.Asc
                                ? query.ThenBy(order.Field)
                                : query.ThenByDescending(order.Field);
                }
            }

            CompleteQuestionnaireDocument[] page =
                query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();

            IEnumerable<CompleteQuestionnaireExportItem> items = page.Select(
                x => new CompleteQuestionnaireExportItem(x));

            return new CompleteQuestionnaireExportView(input.Page, input.PageSize, count, items, input.Order);
        }

        #endregion
    }
}