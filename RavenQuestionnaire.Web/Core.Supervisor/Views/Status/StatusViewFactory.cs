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
            var interviewers = this.GetTeamMembersForViewer(input.ViewerId).Select(u => u.PublicKey);

            var status = SurveyStatus.GetStatusByIdOrDefault(input.StatusId);

            var headers =
                this.surveys.Query(
                    _ =>
                    _.GroupBy(
                        x => x.TemplateId,
                        x => x,
                        (tmplId, questionnaires) =>
                        new TemplateLight(tmplId, questionnaires.FirstOrDefault().QuestionnaireTitle)));

            var items =
                this.surveys.Query(
                    _ =>
                    _.Where(
                        x =>
                        x.Responsible != null && interviewers.Contains(x.Responsible.Id)
                        && (status.PublicId == SurveyStatus.Unknown.PublicId || x.Status.PublicId == status.PublicId)))
                    .GroupBy(x => x.Responsible, x => x, (u, q) => this.BuildItems(u, q));
          
            if (input.Orders.Count == 0)
            {
                input.Orders.Add(new OrderRequestItem() { Direction = OrderDirection.Asc, Field = "Title" });
            }

            items = input.Orders[0].Direction == OrderDirection.Asc
                                                  ? items.OrderBy(i => this.GetOrderValue(i, input.Orders[0].Field))
                                                  : items.OrderByDescending(i => this.GetOrderValue(i, input.Orders[0].Field));

            return new StatusView(input.Page, input.PageSize, status, headers.ToList(), items);
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

        protected StatusViewItem BuildItems(UserLight user, IEnumerable<CompleteQuestionnaireBrowseItem> questionnaires)
        {
            return new StatusViewItem(
                user, questionnaires.GroupBy(_ => _.TemplateId).ToDictionary(x => x.Key, y => y.Count()));
        }
    }
}
