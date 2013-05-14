namespace Core.Supervisor.Views.Status
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    public class StatusViewFactory : BaseUserViewFactory, IViewFactory<StatusViewInputModel, StatusView>
    {
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        public StatusViewFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys,
            IQueryableDenormalizerStorage<UserDocument> users) : base(users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public StatusView Load(StatusViewInputModel input)
        {
            var interviewers = this.GetTeamMembersForViewer(input.ViewerId).Select(u => u.PublicKey).ToList();

            var status = SurveyStatus.GetStatusByIdOrDefault(input.StatusId);

            List<TemplateLight> headers = this.surveys.Query(_ => _.Select(s => new TemplateLight(s.TemplateId, s.QuestionnaireTitle)).Distinct().ToList());

            List<IGrouping<UserLight, CompleteQuestionnaireBrowseItem>> groupedSurveys = 
                this.surveys.Query(_ => _
                    .Where(x => 
                        status.PublicId == SurveyStatus.Unknown.PublicId
                        ? (
                            x.Responsible != null 
                            && interviewers.Contains(x.Responsible.Id)
                        )
                        : (
                            x.Responsible != null 
                            && interviewers.Contains(x.Responsible.Id) 
                            && x.Status.PublicId == status.PublicId
                        ))
                    .GroupBy(x => x.Responsible)
                    .ToList());

            var items = this.BuildItems(groupedSurveys, headers).AsQueryable();

            var retval = new StatusView(input.Page, input.PageSize, 0, status, headers);
            if (input.Orders.Count == 0)
            {
                input.Orders.Add(new OrderRequestItem() { Direction = OrderDirection.Asc, Field = "Title" });
            }

            items = input.Orders[0].Direction == OrderDirection.Asc
                                                  ? items.OrderBy(i => this.GetOrderValue(i, input.Orders[0].Field))
                                                  : items.OrderByDescending(i => this.GetOrderValue(i, input.Orders[0].Field));


            retval.BuildSummary(items, headers);

            retval.TotalCount = items.Count();

            retval.Items = items.ToList();
            return retval;
        }

        private object GetOrderValue(StatusViewItem item, string field)
        {
            if (field == "Title")
            {
                return item.User.Name;
            }

            if (field == "Total")
            {
                return item.Total;
            }

            Guid templateId;
            if (Guid.TryParse(field, out templateId))
            {
                var key = item.Items.Keys.SingleOrDefault(k => k.TemplateId == templateId);
                if (key == null)
                {
                    return 0;
                }

                return item.Items[key];
            }

            return item.User.Name;
        }

        protected IEnumerable<StatusViewItem> BuildItems(IEnumerable<IGrouping<UserLight, CompleteQuestionnaireBrowseItem>> grouped, List<TemplateLight> headers)
        {
            return from templateGroup in grouped
                   let tgroup = templateGroup.GroupBy(g => g.TemplateId).ToDictionary(k => k.Key, v => v.Count())
                   select new StatusViewItem(templateGroup.Key, tgroup, headers);
        }
    }
}
