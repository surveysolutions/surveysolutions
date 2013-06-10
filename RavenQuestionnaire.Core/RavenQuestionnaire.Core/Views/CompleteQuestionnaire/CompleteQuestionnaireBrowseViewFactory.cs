// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireBrowseViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire browse view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Documents;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireBrowseViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CompleteQuestionnaireBrowseViewFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession)
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

            if (input.ResponsibleId != Guid.Empty)
            {
                // filter result by responsible
                query = this.documentSession.Query().Where(x => x.Responsible.Id == input.ResponsibleId);
            }
            else
            {
                query = this.documentSession.Query();
            }

            CompleteQuestionnaireBrowseItem[] page =
                query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToArray();

            return new CompleteQuestionnaireBrowseView(input.Page, input.PageSize, count, page, input.Order);
        }

        #endregion
    }
}