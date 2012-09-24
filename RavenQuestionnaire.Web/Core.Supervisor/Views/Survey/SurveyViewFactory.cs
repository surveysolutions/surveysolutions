// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Denormalizers;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;

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
                (input.UserId == Guid.Empty
                     ? this.documentItemSession.Query()
                     : this.documentItemSession.Query().Where(x => x.Responsible.Id == input.UserId)).GroupBy(
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

        /// <summary>
        /// Builds items
        /// </summary>
        /// <param name="grouped">
        /// The grouped.
        /// </param>
        /// <returns>
        /// List of survey browse items
        /// </returns>
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
                                                    SurveyStatus.Approve.PublicId), templateGroup.Count(
                                                    q =>
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Redo.PublicId));
            }
        }

        #endregion
    }
}