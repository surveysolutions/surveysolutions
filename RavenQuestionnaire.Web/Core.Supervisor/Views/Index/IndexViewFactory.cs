// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IndexViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Core.Supervisor.Views.DenormalizerStorageExtensions;
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
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        /// <summary>
        /// The stat
        /// </summary>
        private readonly IDenormalizerStorage<SupervisorStatisticsItem> stat;

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
            IDenormalizerStorage<UserDocument> users,
            IDenormalizerStorage<SupervisorStatisticsItem> stat)
        {
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
            IEnumerable<Guid> responsibleList;
            UserDocument user;

            if (input.InterviewerId.HasValue)
            {
                user = this.users.GetByGuid(input.InterviewerId.Value);
                responsibleList = new Guid[] { input.InterviewerId.Value };
            }
            else
            {
                user = null;
                responsibleList = this.users.GetTeamMembersForViewer(input.ViewerId).Select(i=>i.PublicKey);
            }

            var all = this.stat.Query()
                .Where(s => s.Surveys.Count > 0)
                .Where(x => responsibleList.Contains(x.User.Id))
                .GroupBy(s => s.Template).ToDictionary(s => s.Key, s => s.ToList());

            var items = BuildStatItems(all, input);

            return new IndexView(input.Page, input.PageSize, items.ToList(), user);
        }

        private IQueryable<IndexViewItem> OrderByItems(IndexInputModel input, IQueryable<IndexViewItem> items)
        {
            if (input.Orders.Count > 0)
            {
                items = input.Orders[0].Direction == OrderDirection.Asc
                            ? items.OrderBy(input.Orders[0].Field)
                            : items.OrderByDescending(
                                input.Orders[0].Field);
            }
            return items;
        }

        private IQueryable<IndexViewItem> BuildStatItems(Dictionary<TemplateLight, List<SupervisorStatisticsItem>> dictionary,IndexInputModel input)
        {
            List<IndexViewItem> result=new List<IndexViewItem>();
            foreach (var kvp in dictionary)
            {
                var allStatuses = SurveyStatus.GetAllStatuses().ToDictionary(x => x, x => 0);
                foreach (var statisticsItem in kvp.Value)
                {
                    allStatuses[statisticsItem.Status] += statisticsItem.Surveys.Count;
                }

                result.Add(new IndexViewItem(
                               kvp.Key.TemplateId,
                               kvp.Key.Title,
                               allStatuses[SurveyStatus.Unassign],
                               allStatuses.Values.Sum(),
                               allStatuses[SurveyStatus.Initial],
                               allStatuses[SurveyStatus.Error],
                               allStatuses[SurveyStatus.Complete],
                               allStatuses[SurveyStatus.Approve],
                               allStatuses[SurveyStatus.Redo]));
            }

            return OrderByItems(input, result.AsQueryable());
        }

        #endregion
    }
}