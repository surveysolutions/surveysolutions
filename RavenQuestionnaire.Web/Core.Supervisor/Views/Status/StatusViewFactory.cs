// -----------------------------------------------------------------------
// <copyright file="StatusViewFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Status
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Core.Supervisor.Views.Summary;

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
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

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
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> surveys,
            IDenormalizerStorage<UserDocument> users)
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
            var interviewers = this.users.Query().Where(u => u.Supervisor != null && u.Supervisor.Id == input.Supervisor.Id).Select(u => u.PublicKey).ToList();
            var status = SurveyStatus.GetAllStatuses().FirstOrDefault(s => s.PublicId == input.StatusId)
                         ?? new SurveyStatus(Guid.Empty, "Any");
            
            var headers = this.surveys.Query().Select(s => new TemplateLight(s.TemplateId, s.QuestionnaireTitle)).Distinct().ToList();

            var items = this.BuildItems(
                    (status.PublicId == Guid.Empty
                         ? this.surveys.Query().Where(
                             x => x.Responsible != null && interviewers.Contains(x.Responsible.Id))
                         : this.surveys.Query().Where(x => x.Responsible != null && interviewers.Contains(x.Responsible.Id)
                                                 && (x.Status.PublicId == status.PublicId))).GroupBy(x => x.Responsible),
                headers).AsQueryable();

            var retval = new StatusView(input.Page, input.PageSize, 0, status, headers);
            if (input.Orders.Count > 0)
            {/*
                items = input.Orders[0].Direction == OrderDirection.Asc
                                                      ? items.OrderBy(input.Orders[0].Field)
                                                      : items.OrderByDescending(input.Orders[0].Field);
              */
            }

            retval.BuildSummary(items, headers);

            retval.TotalCount = items.Count();

            retval.Items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return retval;
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
