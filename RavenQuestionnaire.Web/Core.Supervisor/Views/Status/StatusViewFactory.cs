using System.Linq.Expressions;
using Core.Supervisor.Views.Survey;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

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
        private readonly IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem> surveys;

        public StatusViewFactory(
            IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem> surveys,
            IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        public StatusView Load(StatusViewInputModel input)
        {
#warning here we need to create mapreduce index in oreder to avoid in memory group by
            var interviewers = this.GetTeamMembersForViewer(input.ViewerId).Select(u => u.PublicKey).ToList();

            var status = SurveyStatus.GetStatusByIdOrDefault(input.StatusId);

#warning ReadLayer: Select is not supported on Raven side (fails with NRE)

            List<TemplateLight> headers = this.surveys.QueryAll(q => true).Select(
                s =>
                new TemplateLight(s.TemplateId,
                                  s.QuestionnaireTitle)).Distinct()
                                              .ToList();
            
            Expression<Func<CompleteQuestionnaireBrowseItem, bool>> predicate = (x) => x.Responsible != null && x.Responsible.Id.In(interviewers);

            if (status.PublicId != SurveyStatus.Unknown.PublicId)
            {
                predicate = predicate.AndCondition(x => x.Status.PublicId == status.PublicId);
            }

            var query = this.surveys.QueryAll(predicate);

            List<IGrouping<UserLight, CompleteQuestionnaireBrowseItem>> groupedSurveys = query
                .GroupBy(x => x.Responsible)
                .ToList();

            var items = BuildItems(groupedSurveys).AsQueryable();


            if (input.Orders.Count == 0)
            {
                input.Orders.Add(new OrderRequestItem() {Direction = OrderDirection.Asc, Field = "Title"});
            }

            items = input.Orders[0].Direction == OrderDirection.Asc
                        ? items.OrderBy(i => this.GetOrderValue(i, input.Orders[0].Field))
                        : items.OrderByDescending(i => this.GetOrderValue(i, input.Orders[0].Field));

            return new StatusView(input.Page, input.PageSize, status, headers, items);
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
                return item.GetCount(templateId);
            }

            return item.User.Name;
        }

        protected IEnumerable<StatusViewItem> BuildItems(IEnumerable<IGrouping<UserLight, CompleteQuestionnaireBrowseItem>> grouped)
        {
            return from templateGroup in grouped
                   let tgroup = templateGroup.GroupBy(g => g.TemplateId).ToDictionary(k => k.Key, v => v.Count())
                   select new StatusViewItem(templateGroup.Key, tgroup);
        }
    }
}
