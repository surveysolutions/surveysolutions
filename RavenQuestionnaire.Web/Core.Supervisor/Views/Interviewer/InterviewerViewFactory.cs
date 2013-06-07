// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterviewerViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The interviewer view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;

namespace Core.Supervisor.Views.Interviewer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The interviewer view factory.
    /// </summary>
    public class InterviewerViewFactory : IViewFactory<InterviewerInputModel, InterviewerView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentItemSession;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InterviewerViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        public InterviewerViewFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentSession,
            IQueryableDenormalizerStorage<UserDocument> users)
        {
            this.documentItemSession = documentSession;
            this.users = users;
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
        /// The RavenQuestionnaire.Core.Views.User.InterviewerView.
        /// </returns>
        public InterviewerView Load(InterviewerInputModel input)
        {
            UserDocument user = this.users.Query(_ => _.FirstOrDefault(u => u.PublicKey == input.InterviwerId));

            return this.documentItemSession.Query(queryable =>
            {
                var items = new List<InterviewerGroupView>();

                var docs = queryable.Where(q => q.Responsible != null && q.Responsible.Id == user.PublicKey);

                if (input.TemplateId.HasValue)
                {
                    InterviewerGroupView interviewerGroupView = this.SelectItems(input.TemplateId.Value, docs, input);
                    if (interviewerGroupView.Items.Count > 0)
                    {
                        items.Add(interviewerGroupView);
                    }
                }
                else
                {
                    docs.GroupBy(
                        t => t.TemplateId,
                        t => t,
                        (templateId, questionnaires) => this.SelectItems(templateId, questionnaires, input))
                        .Where(_ => _.Items.Any())
                        .ToList()
                        .ForEach(items.Add);
                }

                return new InterviewerView(user.UserName, user.PublicKey, items.ToList());
            });
        }

        #endregion

        #region Methods

        /// <summary>
        /// The select items.
        /// </summary>
        /// <param name="templateId">
        /// The template id.
        /// </param>
        /// <param name="docs">
        /// The docs.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.User.InterviewerGroupView.
        /// </returns>
        private InterviewerGroupView SelectItems(
            Guid templateId, IEnumerable<CompleteQuestionnaireBrowseItem> docs, InterviewerInputModel input)
        {
            var query = docs.Where(t => t.TemplateId == templateId);
            if (input.Orders.Any())
            {
                var o =
                    query.Where(t => t.FeaturedQuestions.Any())
                        .SelectMany(t => t.FeaturedQuestions.Select(y => y.PublicKey.ToString()));
                if (o.Contains(input.Orders[0].Field))
                {
                    Func<CompleteQuestionnaireBrowseItem, string> order =
                        t =>
                        t.FeaturedQuestions.Where(y => y.PublicKey.ToString() == input.Orders[0].Field)
                         .Select(x => x.Answer.ToString())
                         .FirstOrDefault();

                    query = input.Orders[0].Direction == OrderDirection.Asc
                                ? query.OrderBy(_ => order)
                                : query.OrderByDescending(_ => order);
                }
                else
                {
                    query = query.AsQueryable().OrderUsingSortExpression(input.Order).AsQueryable();
                }
            }

            var items = query.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return new InterviewerGroupView(
                templateId,
                items.FirstOrDefault() == null ? string.Empty : items.FirstOrDefault().QuestionnaireTitle, 
                items, 
                input.Order, 
                input.Page, 
                input.PageSize, 
                query.Count());
        }

        #endregion
    }
}