// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyGroupViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey group view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Denormalizers;
    using RavenQuestionnaire.Core.Entities;
    using RavenQuestionnaire.Core.Utility;
    using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

    /// <summary>
    /// The survey group view factory.
    /// </summary>
    public class SurveyGroupViewFactory : IViewFactory<SurveyGroupInputModel, SurveyGroupView>
    {
        #region Fields

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
        /// <param name="status"></param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.Survey.SurveyGroupView.
        /// </returns>
        public SurveyGroupView Load(SurveyGroupInputModel input)
        {
            int count = this.documentItemSession.Query().Count(x => x.TemplateId == input.Id);
            if (count == 0)
            {
                return new SurveyGroupView(
                    input.Page, 
                    input.PageSize, 
                    String.Empty, // where is this item used?   
                    0, 
                    new CompleteQuestionnaireBrowseItem[0], 
                    input.Id);
            }
            SurveyStatus st = SurveyStatus.IsValidStatus(input.Status);
            IQueryable<CompleteQuestionnaireBrowseItem> items = (st == null)
                                                                    ? this.documentItemSession.Query().Where(
                                                                        v => v.TemplateId == input.Id)
                                                                    : this.documentItemSession.Query().Where(
                                                                        v => v.TemplateId == input.Id).Where(
                                                                            v => v.Status.PublicId == st.PublicId);
            
            if (input.QuestionnaireId != Guid.Empty)
            {
                items = items.Where(t => t.CompleteQuestionnaireId == input.QuestionnaireId);
            }

            if (input.Orders.Count > 0)
            {
                items = this.DefineOrderBy(items, input);
            }

            items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);

            return new SurveyGroupView(
                input.Page, 
                input.PageSize,

                items.FirstOrDefault() != null ? items.FirstOrDefault().QuestionnaireTitle : input.Id.ToString(), //Fix it!
                
                count, 
                items, 
                input.Id);
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
            List<string> o = query.SelectMany(t => t.FeaturedQuestions).Select(y => y.QuestionText).Distinct().ToList();
            if (o.Contains(input.Orders[0].Field))
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(
                                t =>
                                t.FeaturedQuestions.Where(y => y.QuestionText == input.Orders[0].Field).Select(
                                    x => x.AnswerValue).FirstOrDefault())
                            : query.OrderByDescending(
                                t =>
                                t.FeaturedQuestions.Where(y => y.QuestionText == input.Orders[0].Field).Select(
                                    x => x.AnswerValue).FirstOrDefault());
            }
            else
            {
                if (input.Orders[0].Field.Contains("Responsible"))
                {
                    IQueryable<CompleteQuestionnaireBrowseItem> usersnull = query.Where(t => t.Responsible == null);
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
                                ? usersnull.Union(contains)
                                : contains.Union(usersnull);
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