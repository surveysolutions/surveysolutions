// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.Survey
{
    using System.Collections.Generic;
    using System.Linq;
    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Entities;
    using RavenQuestionnaire.Core.Utility;
    
    /// <summary>
    /// The survey view factory.
    /// </summary>
    public class SurveyViewFactory : IViewFactory<SurveyViewInputModel, SurveyBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<SurveyBrowseItem> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        public SurveyViewFactory(IDenormalizerStorage<SurveyBrowseItem> documentItemSession)
        {
            this.documentItemSession = documentItemSession;
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
        /// The RavenQuestionnaire.Core.Views.Survey.SurveyBrowseView.
        /// </returns>
        public SurveyBrowseView Load(SurveyViewInputModel input)
        {
            var count = this.documentItemSession.Query().Count();
            if (count == 0) 
                return new SurveyBrowseView(input.Page, input.PageSize, count, new List<SurveyBrowseItem>());
            IQueryable<SurveyBrowseItem> query = this.documentItemSession.Query();
            if (input.Orders.Count > 0)
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(input.Orders[0].Field)
                            : query.OrderByDescending(input.Orders[0].Field);
            query = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new SurveyBrowseView(input.Page, input.PageSize, count, query);
        }

        #endregion
    }
}