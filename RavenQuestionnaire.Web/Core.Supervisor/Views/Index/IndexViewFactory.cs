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

    using Core.Supervisor.DenormalizerStorageItem;

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
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        /// <summary>
        /// The stat
        /// </summary>
        private readonly IQueryableDenormalizerStorage<SupervisorStatisticsItem> stat;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexViewFactory"/> class.
        /// </summary>
        /// <param name="surveys">
        /// The document item session.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        /// <param name="stat">
        /// The stat.
        /// </param>
        public IndexViewFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys,
            IQueryableDenormalizerStorage<UserDocument> users,
            IQueryableDenormalizerStorage<SupervisorStatisticsItem> stat)
        {
            this.surveys = surveys;
            this.users = users;
            this.stat = stat;
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

            var all = this.stat.Query()
                .Where(s => s.Surveys.Count > 0)
                .Where(x => input.UserId == Guid.Empty ? true : (x.User.Id == input.UserId))
                .GroupBy(s => s.Template).ToDictionary(s => s.Key, s => s.ToList());

            var items = this.BuildStatItems(all).AsQueryable();

            /* var items = this.BuildItems((input.UserId == Guid.Empty
                     ? this.surveys.Query()
                     : this.surveys.Query().Where(
                         x => x.Responsible != null && (x.Responsible.Id == input.UserId))).GroupBy(x => x.TemplateId)).
                    AsQueryable();*/

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
                items.Sum(x => x.Completed),
                items.Sum(x => x.Approved),
                items.Sum(x => x.Redo));

            retval.TotalCount = items.Count();

            retval.Items = items.ToList();
            //items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return retval;
        }

        /// <summary>
        /// Builds items
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary.
        /// </param>
        /// <returns>
        /// List of survey browse items
        /// </returns>
        private IEnumerable<IndexViewItem> BuildStatItems(Dictionary<TemplateLight, List<SupervisorStatisticsItem>> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                var allStatuses = SurveyStatus.GetAllStatuses().ToDictionary(x => x, x => 0);
                foreach (var statisticsItem in kvp.Value)
                {
                    allStatuses[statisticsItem.Status] += statisticsItem.Surveys.Count;
                }

                yield return new IndexViewItem(
                        kvp.Key.TemplateId,
                        kvp.Key.Title,
                        allStatuses[SurveyStatus.Unassign],
                        allStatuses.Values.Sum(),
                        allStatuses[SurveyStatus.Initial],
                        allStatuses[SurveyStatus.Error],
                        allStatuses[SurveyStatus.Complete],
                        allStatuses[SurveyStatus.Approve],
                        allStatuses[SurveyStatus.Redo]);
            }
        }

        #endregion
    }
}