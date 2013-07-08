using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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
        private readonly IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem> documentItemSession;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

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
            IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem> documentSession,
            IQueryableReadSideRepositoryReader<UserDocument> users)
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
                var items = new InterviewerView(user.UserName, user.PublicKey, new List<InterviewerGroupView>());

                IQueryable<CompleteQuestionnaireBrowseItem> docs =
                    queryable.Where(q => q.Responsible != null && q.Responsible.Id == user.PublicKey);

                if (input.TemplateId.HasValue)
                {
                    InterviewerGroupView interviewerGroupView = this.SelectItems(input.TemplateId.Value, docs, input);
                    if (interviewerGroupView.Items.Count > 0)
                    {
                        items.Items.Add(interviewerGroupView);
                    }
                }
                else
                {
                    IQueryable<IGrouping<Guid, CompleteQuestionnaireBrowseItem>> gr = docs.GroupBy(t => t.TemplateId);
                    foreach (InterviewerGroupView interviewerGroupView in
                        gr.ToList().Select(template => this.SelectItems(template.Key, docs, input)).Where(
                            interviewerGroupView => interviewerGroupView.Items.Count > 0))
                    {
                        items.Items.Add(interviewerGroupView);
                    }
                }

                return items;
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
            Guid templateId, IQueryable<CompleteQuestionnaireBrowseItem> docs, InterviewerInputModel input)
        {
            int count = docs.Where(t => t.TemplateId == templateId).Count();
            if (count == 0)
            {
                return new InterviewerGroupView(
                    templateId, 
                    string.Empty, 
                    new List<CompleteQuestionnaireBrowseItem>(), 
                    input.Order, 
                    input.Page, 
                    input.PageSize, 
                    count);
            }

            docs = docs.Where(t => t.TemplateId == templateId);
            if (input.Orders.Count > 0)
            {
                List<string> o =
                    Queryable.Distinct<string>(docs.Where(t => t.FeaturedQuestions.Count() != 0).SelectMany(t => t.FeaturedQuestions).Select(
                            y => y.PublicKey.ToString())).ToList();
                if (o.Contains(input.Orders[0].Field))
                {
                    docs = input.Orders[0].Direction == OrderDirection.Asc
                               ? docs.OrderBy(
                                   t =>
                                   t.FeaturedQuestions.Where(y => y.PublicKey.ToString() == input.Orders[0].Field).
                                       Select(x => x.Answer.ToString()).FirstOrDefault())
                               : docs.OrderByDescending(
                                   t =>
                                   t.FeaturedQuestions.Where(y => y.PublicKey.ToString() == input.Orders[0].Field).
                                       Select(x => x.Answer.ToString()).FirstOrDefault());
                }
                else
                {
                    docs = input.Orders[0].Direction == OrderDirection.Asc
                               ? docs.OrderBy(input.Orders[0].Field)
                               : docs.OrderByDescending(input.Orders[0].Field);
                }
            }

            docs = docs.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize);
            return new InterviewerGroupView(
                templateId, 
                docs.ToList().Count > 0 ? docs.ToList().FirstOrDefault().QuestionnaireTitle : string.Empty, 
                docs.ToList(), 
                input.Order, 
                input.Page, 
                input.PageSize, 
                count);
        }

        #endregion
    }
}