// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using Main.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    using System.Collections.Generic;
    using System.Linq;
    using Main.Core.Entities;
    using Main.Core.Utility;

    using Main.Core.Entities;
    using Main.Core.Utility;

    using RavenQuestionnaire.Core.Denormalizers;
    
    /// <summary>
    /// The survey view factory.
    /// </summary>
    public class SurveyViewFactory : IViewFactory<SurveyViewInputModel, SurveyBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        public SurveyViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
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
         /*   var count = this.documentItemSession.Query().Count();
            if (count == 0) 
                return new SurveyBrowseView(input.Page, input.PageSize, count, new List<SurveyBrowseItem>());
            IQueryable<SurveyBrowseItem> query = this.documentItemSession.Query();
            if (input.Orders.Count > 0)
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(input.Orders[0].Field)
                            : query.OrderByDescending(input.Orders[0].Field);
            query = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new SurveyBrowseView(input.Page, input.PageSize, count, query);*/
            var questionnairesGroupedByTemplate =
              BuildItems(
                  this.documentItemSession.Query().GroupBy(
                      x => x.TemplateId)).AsQueryable();

            var retval = new SurveyBrowseView(input.Page, input.PageSize, 0, new List<SurveyBrowseItem>());
            if (input.Orders.Count > 0)
            {
                questionnairesGroupedByTemplate = input.Orders[0].Direction == OrderDirection.Asc
                                                      ? questionnairesGroupedByTemplate.OrderBy(input.Orders[0].Field)
                                                      : questionnairesGroupedByTemplate.OrderByDescending(
                                                          input.Orders[0].Field);
            }

            retval.Items =
                questionnairesGroupedByTemplate.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return retval;
        }
        protected IEnumerable<SurveyBrowseItem> BuildItems(IQueryable<IGrouping<Guid, CompleteQuestionnaireBrowseItem>> grouped)
        {
            foreach (var templateGroup in grouped)
            {
                yield
                    return new SurveyBrowseItem(templateGroup.Key,
                                                templateGroup.FirstOrDefault().QuestionnaireTitle,
                                                templateGroup.Count(
                                                    q =>
                                                    q.Responsible == null),
                                                templateGroup.Count(),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Initial.PublicId),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Status.PublicId == SurveyStatus.Error.PublicId),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Complete.PublicId),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Approve.PublicId));
            }
        }

        #endregion
    }
}