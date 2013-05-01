// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssignmentViewFactory.cs" company="">
//   
// </copyright>
// <summary>
//   The survey group view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Assignment
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.Questionnaire;

    using Main.DenormalizerStorage;

    /// <summary>
    /// The survey group view factory.
    /// </summary>
    public class AssignmentViewFactory : IViewFactory<AssignmentInputModel, AssignmentView>
    {
        #region Constants and Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// The templates.
        /// </summary>
        private readonly IDenormalizerStorage<QuestionnaireBrowseItem> templates;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssignmentViewFactory"/> class.
        /// </summary>
        /// <param name="surveys">
        /// The document item session.
        /// </param>
        public AssignmentViewFactory(
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys,
            IDenormalizerStorage<QuestionnaireBrowseItem> templates,
            IDenormalizerStorage<UserDocument> users)
        {
            this.surveys = surveys;
            this.templates = templates;
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
        /// The RavenQuestionnaire.Core.Views.Survey.AssignmentView.
        /// </returns>
        public AssignmentView Load(AssignmentInputModel input)
        {
            var view = new AssignmentView(input.Page, input.PageSize, 0);
            view.Template = !input.TemplateId.HasValue
                            ? null
                            : this.templates.GetByGuid(input.TemplateId.Value).GetTemplateLight();

            view.User = !input.InterviewerId.HasValue
                            ? null
                            : this.users.GetByGuid(input.InterviewerId.Value).GetUseLight();

            view.Status =  SurveyStatus.Unknown;

            if (input.Statuses != null && input.Statuses.Count > 0)
            {
                view.Status = SurveyStatus.GetStatusByIdOrDefault(input.Statuses.First());
            }

            IQueryable<CompleteQuestionnaireBrowseItem> items = (view.Status.PublicId == SurveyStatus.Unknown.PublicId
                ? this.surveys.Query()
                : this.surveys.Query().Where(v => v.Status.PublicId == view.Status.PublicId))
                .OrderByDescending(t => t.CreationDate);

            if (input.TemplateId.HasValue)
            {
                items = items.Where(x => (x.TemplateId == input.TemplateId));
            }

            if (input.IsNotAssigned)
            {
                items = items.Where(t => t.Responsible == null);
            }
            else if (input.InterviewerId.HasValue)
            {
                items = items.Where(t => t.Responsible != null).Where(x => x.Responsible.Id == input.InterviewerId);
            }

            if (input.QuestionnaireId.HasValue)
            {
                items = items.Where(t => t.CompleteQuestionnaireId == input.QuestionnaireId);
            }

            view.TotalCount = items.Count();

            if (input.Orders.Count > 0)
            {
                items = this.DefineOrderBy(items, input);
            }

            view.SetItems(items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize));

            return view;
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
            IQueryable<CompleteQuestionnaireBrowseItem> query, AssignmentInputModel input)
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