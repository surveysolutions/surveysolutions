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

    public class IndexViewFactory : BaseUserViewFactory, IViewFactory<IndexInputModel, IndexView>
    {
        private readonly IQueryableDenormalizerStorage<SupervisorStatisticsItem> stat;

        public IndexViewFactory(IQueryableDenormalizerStorage<UserDocument> users,
            IQueryableDenormalizerStorage<SupervisorStatisticsItem> stat) : base(users)
        {
            this.stat = stat;
        }

        public IndexView Load(IndexInputModel input)
        {
            IEnumerable<Guid> responsibleList;
            UserDocument user;

            if (input.InterviewerId.HasValue)
            {
                user = this.users.GetById(input.InterviewerId.Value);
                responsibleList = new Guid[] { input.InterviewerId.Value };
            }
            else
            {
                user = null;
                responsibleList = this.GetTeamMembersForViewer(input.ViewerId).Select(i=>i.PublicKey);
            }

            var all = this.stat.Query(_ => _
                .Where(s => s.Surveys.Count > 0)
                .Where(x => responsibleList.Contains(x.User.Id))
                .GroupBy(s => s.Template)
                .ToDictionary(s => s.Key, s => s.ToList()));

            var items = BuildStatItems(all, input);

            return new IndexView(input.Page, input.PageSize, items.ToList(), user);
        }

        private IEnumerable<IndexViewItem> OrderByItems(IndexInputModel input, IQueryable<IndexViewItem> items)
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

        private IEnumerable<IndexViewItem> BuildStatItems(Dictionary<TemplateLight, List<SupervisorStatisticsItem>> dictionary,IndexInputModel input)
        {
            var result=new List<IndexViewItem>();
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
    }
}