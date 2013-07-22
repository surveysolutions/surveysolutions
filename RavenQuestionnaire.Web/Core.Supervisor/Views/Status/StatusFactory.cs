using System.Linq.Expressions;
using Core.Supervisor.DenormalizerStorageItem;
using Core.Supervisor.Views.Survey;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

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
        private readonly IQueryableReadSideRepositoryReader<SummaryItem> interviews;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> template;
        public StatusViewFactory(
            IQueryableReadSideRepositoryReader<SummaryItem> interviews, IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> template,
            IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.interviews = interviews;
            this.template = template;
            this.users = users;
        }

        public StatusView Load(StatusViewInputModel input)
        {
#warning here we need to create mapreduce index in oreder to avoid in memory group by

            var viewer = users.GetById(input.ViewerId);
            var status = SurveyStatus.GetStatusByIdOrDefault(input.StatusId);

#warning ReadLayer: Select is not supported on Raven side (fails with NRE)

            List<TemplateLight> headers = this.template.QueryAll(q => true).Select(
                s =>
                new TemplateLight(s.QuestionnaireId,
                                  s.Title)).ToList();

            Expression<Func<SummaryItem, bool>> predicate = (x) => true;

            if (viewer.Roles.Contains(UserRoles.Headquarter))
            {
                predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == null);
            }
            else if (viewer.Roles.Contains(UserRoles.Supervisor))
            {
                predicate = predicate.AndCondition(x => x.ResponsibleSupervisorId == input.ViewerId);
            }

            var query = this.interviews.QueryAll(predicate);

            var items = query
                .GroupBy(x => x.ResponsibleId, y => y,
                         (x, y) =>
                         new StatusViewItem(new UserLight(x, y.FirstOrDefault().ResponsibleName),
                                            headers.ToDictionary(k => k.TemplateId,
                                                                 k => SumByTemplate(y, k.TemplateId, status))));
            var orders = input.Orders.ToList();
            if (orders.Count() == 0)
            {
                orders.Add(new OrderRequestItem() { Direction = OrderDirection.Asc, Field = "Title" });
            }

            items = orders[0].Direction == OrderDirection.Asc
                        ? items.OrderBy(i => this.GetOrderValue(i, orders[0].Field))
                        : items.OrderByDescending(i => this.GetOrderValue(i, orders[0].Field));

            return new StatusView(input.Page, input.PageSize, status, headers, items);
        }

        private bool IsStatusDefined(SurveyStatus status)
        {
            return status.PublicId != SurveyStatus.Unknown.PublicId;
        }

        private int SumByTemplate(IEnumerable<SummaryItem> items, Guid templateId, SurveyStatus status)
        {
            return items.Where(i => i.TemplateId == templateId).Sum(i => GetCountByStatus(i, status));
        }

        private int GetCountByStatus(SummaryItem item, SurveyStatus status)
        {
            if (!IsStatusDefined(status))
                return item.TotalCount;
            if (status.PublicId == SurveyStatus.Approve.PublicId)
                return item.ApprovedCount;
            if (status.PublicId == SurveyStatus.Complete.PublicId)
                return item.CompletedCount;
            if (status.PublicId == SurveyStatus.Error.PublicId)
                return item.CompletedWithErrorsCount;
            if (status.PublicId == SurveyStatus.Initial.PublicId)
                return item.InitialCount;
            if (status.PublicId == SurveyStatus.Redo.PublicId)
                return item.RedoCount;
            if (status.PublicId == SurveyStatus.Unassign.PublicId)
                return item.UnassignedCount;

            throw new ArgumentException("undefined status");
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
    }
}
