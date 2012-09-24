// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyGroupViewFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The survey group view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Denormalizers;
    using Main.Core.Entities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The survey group view factory.
    /// </summary>
    public class SurveyGroupViewFactory : IViewFactory<SurveyGroupInputModel, SurveyGroupView>
    {
        #region Constants and Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyGroupViewFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        public SurveyGroupViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
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
        /// The RavenQuestionnaire.Core.Views.Survey.SurveyGroupView.
        /// </returns>
        public SurveyGroupView Load(SurveyGroupInputModel input)
        {
            int count = this.documentItemSession.Query().Count(x => x.TemplateId == input.Id);
            string title = string.Empty;
            CompleteQuestionnaireBrowseItem template =
                this.documentItemSession.Query().Where(v => v.TemplateId == input.Id).FirstOrDefault();
            if (template != null)
            {
                title = template.QuestionnaireTitle;
            }

            if (count == 0)
            {
                return new SurveyGroupView(
                    input.Page, input.PageSize, title, 0, new CompleteQuestionnaireBrowseItem[0], input.Id);
            }
            var statuses = input.Statuses == null
                               ? new List<Guid>()
                               : input.Statuses.Select(Guid.Parse).ToList();
            IQueryable<CompleteQuestionnaireBrowseItem> items =
                statuses.Count == 0
                ? this.documentItemSession.Query().Where(v => v.TemplateId == input.Id)
                : this.documentItemSession.Query().Where(v => v.TemplateId == input.Id)
                                                  .Where(v => statuses.Contains(v.Status.PublicId));
            if (input.IsNotAssigned)
            {
                items = items.Where(t => t.Responsible == null);
            }

            if (input.QuestionnaireId != Guid.Empty)
            {
                items = items.Where(t => t.CompleteQuestionnaireId == input.QuestionnaireId);
            }

            if (input.Orders.Count > 0)
            {
                items = this.DefineOrderBy(items, input);
            }

            items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new SurveyGroupView(input.Page, input.PageSize, title, count, items, input.Id);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The define order by.
        /// </summary>
        /// <param name="query">
        /// The query.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The System.Linq.IQueryable`1[T -&gt; RavenQuestionnaire.Core.Views.CompleteQuestionnaire.CompleteQuestionnaireBrowseItem].
        /// </returns>
        private IQueryable<CompleteQuestionnaireBrowseItem> DefineOrderBy(
            IQueryable<CompleteQuestionnaireBrowseItem> query, SurveyGroupInputModel input)
        {
            List<string> o = query.SelectMany(t => t.FeaturedQuestions).Select(y => y.Title).Distinct().ToList();
            if (o.Contains(input.Orders[0].Field))
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(
                                t =>
                                t.FeaturedQuestions.Where(y => y.Title == input.Orders[0].Field).Select(
                                    x => x.Answer.ToString()).FirstOrDefault())
                            : query.OrderByDescending(
                                t =>
                                t.FeaturedQuestions.Where(y => y.Title == input.Orders[0].Field).Select(
                                    x => x.Answer.ToString()).FirstOrDefault());
            }
            else
            {
                if (input.Orders[0].Field.Contains("Responsible"))
                {
                    IQueryable<CompleteQuestionnaireBrowseItem> usersNull = query.Where(t => t.Responsible == null);
                    IOrderedQueryable<CompleteQuestionnaireBrowseItem> contains = input.Orders[0].Direction
                                                                                  == OrderDirection.Asc
                                                                                      ? query.Where(
                                                                                          t => t.Responsible != null).
                                                                                            OrderBy(
                                                                                                input.Orders[0].Field)
                                                                                      : query.Where(
                                                                                          t => t.Responsible != null).
                                                                                            OrderByDescending(
                                                                                                input.Orders[0].Field);
                    query = (input.Orders[0].Direction == OrderDirection.Asc)
                                ? usersNull.Union(contains)
                                : contains.Union(usersNull);
                }
                else
                {
                    query = input.Orders[0].Direction == OrderDirection.Asc
                                ? query.OrderBy(input.Orders[0].Field)
                                : query.OrderByDescending(input.Orders[0].Field);
                }
            }

            return query;
        }

        #endregion
    }
}