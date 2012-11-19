// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.DenormalizerStorage;

namespace Core.Supervisor.Views.Index
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

    /// <summary>
    /// The survey view factory.
    /// </summary>
    public class IndexViewFactory : IViewFactory<IndexInputModel, IndexView>
    {
        #region Fields

        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexViewFactory"/> class.
        /// </summary>
        /// <param name="surveys">
        /// The document item session.
        /// </param>
        public IndexViewFactory(IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys,
            IDenormalizerStorage<UserDocument> users)
        {
            this.surveys = surveys;
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
        /// The RavenQuestionnaire.Core.Views.Survey.IndexView.
        /// </returns>
        public IndexView Load(IndexInputModel input)
        {
            UserDocument user = null;
            if (input.UserId != Guid.Empty)
            {
                user = this.users.Query().FirstOrDefault(u => u.PublicKey == input.UserId);
            }

            var items = this.BuildItems((input.UserId == Guid.Empty
                     ? this.surveys.Query()
                     : this.surveys.Query().Where(
                         x => x.Responsible != null && (x.Responsible.Id == input.UserId))).GroupBy(x => x.TemplateId)).
                    AsQueryable();

            var retval = new IndexView(input.Page, input.PageSize, 0, new List<IndexViewItem>(), user);
            if (input.Orders.Count > 0)
            {
                items = input.Orders[0].Direction == OrderDirection.Asc
                                                      ? items.OrderBy(input.Orders[0].Field)
                                                      : items.OrderByDescending(
                                                          input.Orders[0].Field);
            }
            retval.Summary = new IndexViewItem(
                Guid.Empty,
                "Summary",
                items.Sum(x => x.Unassigned),
                items.Sum(x => x.Total),
                items.Sum(x => x.Initial),
                items.Sum(x => x.Error),
                items.Sum(x => x.Complete),
                items.Sum(x => x.Approve),
                items.Sum(x => x.Redo));

            retval.TotalCount = items.Count();

            retval.Items =
                items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
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
        protected IEnumerable<IndexViewItem> BuildItems(IQueryable<IGrouping<Guid, CompleteQuestionnaireBrowseItem>> grouped)
        {
            foreach (var templateGroup in grouped)
            {
                yield
                    return new IndexViewItem(templateGroup.Key,
                                                templateGroup.FirstOrDefault().QuestionnaireTitle,
                                                templateGroup.Count(
                                                    q =>
                                                    q.Responsible == null),
                                                templateGroup.Count(),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Responsible != null &&
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Initial.PublicId),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Responsible != null &&
                                                    q.Status.PublicId == SurveyStatus.Error.PublicId),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Responsible != null &&
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Complete.PublicId),
                                                templateGroup.Count(
                                                    q =>
                                                    q.Responsible != null &&
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Approve.PublicId), 
                                                templateGroup.Count(
                                                    q =>
                                                    q.Responsible != null &&
                                                    q.Status.PublicId ==
                                                    SurveyStatus.Redo.PublicId));
            }
        }

        #endregion
    }
}