// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CQGroupedBrowseFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The cq grouped browse factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Main.DenormalizerStorage;

namespace Core.CAPI.Views.Grouped
{
    /// <summary>
    /// The cq grouped browse factory.
    /// </summary>
    public class CQGroupedBrowseFactory : IViewFactory<CQGroupedBrowseInputModel, CQGroupedBrowseView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CQGroupedBrowseFactory"/> class.
        /// </summary>
        /// <param name="documentItemSession">
        /// The document item session.
        /// </param>
        /// <param name="documentGroupSession">
        /// The document group session.
        /// </param>
        public CQGroupedBrowseFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession)
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
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Grouped.CQGroupedBrowseView.
        /// </returns>
        public CQGroupedBrowseView Load(CQGroupedBrowseInputModel input)
        {
            var questionnairesGroupedByTemplate =
                this.documentItemSession.Query().ToList()/*.Where(BuildPredicate(input)).GroupBy(x => x.TemplateId)*/;

            var retval = new CQGroupedBrowseView(0, 100, 100, new List<CQGroupItem>());

            CQGroupItem item = new CQGroupItem(1, 100, questionnairesGroupedByTemplate.Count,
                                                   "", Guid.NewGuid());
            item.Items = questionnairesGroupedByTemplate;
            retval.Groups.Add(item);
           /* foreach (var templateGroup in questionnairesGroupedByTemplate)
            {

                CQGroupItem item = new CQGroupItem(1, 100, templateGroup.Count(),
                                                   templateGroup.FirstOrDefault().QuestionnaireTitle, templateGroup.Key);
                item.Items = templateGroup.ToList();
                retval.Groups.Add(item);
            }*/

            return retval;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The build predicate.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <param name="surveyId">
        /// The survey id.
        /// </param>
        /// <returns>
        /// The System.Linq.Expressions.Expression`1[TDelegate -&gt; System.Func`2[T -&gt; RavenQuestionnaire.Core.Views.CompleteQuestionnaire.CompleteQuestionnaireBrowseItem, TResult -&gt; System.Boolean]].
        /// </returns>
        protected Expression<Func<CompleteQuestionnaireBrowseItem, bool>> BuildPredicate(
            CQGroupedBrowseInputModel input)
        {
            IList<Expression<Func<CompleteQuestionnaireBrowseItem, bool>>> predicats =
                new List<Expression<Func<CompleteQuestionnaireBrowseItem, bool>>>();
            predicats.Add((q) => q.Status.PublicId != SurveyStatus.Approve.PublicId);
            if (input.InterviewerId.HasValue)
            {
                predicats.Add((q) => q.Responsible != null && q.Responsible.Id == input.InterviewerId);
            }

            return this.AndAll(predicats);
        }

        /// <summary>
        /// The and all.
        /// </summary>
        /// <param name="expressions">
        /// The expressions.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.Expressions.Expression`1[TDelegate -&gt; System.Func`2[T -&gt; T, TResult -&gt; System.Boolean]].
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        private Expression<Func<T, bool>> AndAll<T>(IEnumerable<Expression<Func<T, bool>>> expressions)
        {
            if (expressions == null)
            {
                throw new ArgumentNullException("expressions");
            }

            if (!expressions.Any())
            {
                return t => true;
            }
            Expression<Func<T, bool>> result = c => true;
            foreach (Expression<Func<T, bool>> expression in expressions)
            {
                var prefix = result.Compile();
                var compiledExp = expression.Compile();
                result = c => prefix(c) && compiledExp(c);
            }
            return result;
            /*    Type delegateType =
                typeof (Func<,>).GetGenericTypeDefinition().MakeGenericType(new[] {typeof (T), typeof (bool)});
        
            Expression combined = expressions.Select(e => e.Body).Aggregate(Expression.AndAlso);
            return (Expression<Func<T, bool>>) Expression.Lambda(combined,expressions.FirstOrDefault().Parameters);*/
        }

        #endregion
    }
}