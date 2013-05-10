// -----------------------------------------------------------------------
// <copyright file="StatusViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Core.Supervisor.Views.DenormalizerStorageExtensions;

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

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StatusViewFactory : IViewFactory<StatusViewInputModel, StatusView>
    {
        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusViewFactory"/> class.
        /// </summary>
        /// <param name="surveys">
        /// The surveys.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        public StatusViewFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys,
            IQueryableDenormalizerStorage<UserDocument> users)
        {
            this.surveys = surveys;
            this.users = users;
        }

        /// <summary>
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// </returns>
        public StatusView Load(StatusViewInputModel input)
        {
            var interviewers = this.users.GetTeamMembersForViewer(input.ViewerId).Select(u => u.PublicKey).ToList();

            var status = SurveyStatus.GetStatusByIdOrDefault(input.StatusId);

            var headers = this.surveys.Query().Select(s => new TemplateLight(s.TemplateId, s.QuestionnaireTitle)).Distinct().ToList();

            var items = this.BuildItems(
                    (status.PublicId == SurveyStatus.Unknown.PublicId
                         ? this.surveys.Query().Where(
                             x => x.Responsible != null && interviewers.Contains(x.Responsible.Id))
                         : this.surveys.Query().Where(x => x.Responsible != null && interviewers.Contains(x.Responsible.Id)
                                                 && (x.Status.PublicId == status.PublicId))).GroupBy(x => x.Responsible),
                headers).AsQueryable();

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

            retval.Items = items.ToList();//.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return retval;
        }

        /// <summary>
        /// Value order
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <param name="field">
        /// The field.
        /// </param>
        /// <returns>
        /// Field value
        /// </returns>
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

        /// <summary>
        /// Builds items
        /// </summary>
        /// <param name="grouped">
        /// The grouped.
        /// </param>
        /// <param name="headers">
        /// The list of templates
        /// </param>
        /// <returns>
        /// The build items.
        /// </returns>
        protected IEnumerable<StatusViewItem> BuildItems(IQueryable<IGrouping<UserLight, CompleteQuestionnaireBrowseItem>> grouped, List<TemplateLight> headers)
        {
            return from templateGroup in grouped
                   let tgroup = templateGroup.GroupBy(g => g.TemplateId).ToDictionary(k => k.Key, v => v.Count())
                   select new StatusViewItem(templateGroup.Key, tgroup, headers);
        }
    }
}
