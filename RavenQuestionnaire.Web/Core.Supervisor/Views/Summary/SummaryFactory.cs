// -----------------------------------------------------------------------
// <copyright file="SummaryFactory.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Core.Supervisor.Views.Summary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Core.Supervisor.Views.Index;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryFactory : IViewFactory<SummaryInputModel, SummaryView>
    {
        /// <summary>
        /// The document item session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> survey;

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryFactory"/> class.
        /// </summary>
        /// <param name="survey">
        /// The survey.
        /// </param>
        /// <param name="users">
        /// The users.
        /// </param>
        public SummaryFactory(
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> survey, IDenormalizerStorage<UserDocument> users)
        {
            this.survey = survey;
            this.users = users;
        }

        /// <summary>
        /// Summary view factory load method
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// Summary view
        /// </returns>
        public SummaryView Load(SummaryInputModel input)
        {
            var items = new List<SummaryViewItem>();
            var interviewers = this.users.Query()
                .Where(u => u.Supervisor != null && u.Supervisor.Id == input.Supervisor.Id)
                .Select(u => new UserLight(u.PublicKey, u.UserName)).ToList();

            var standard = this.survey.Query()
                .Where(x => x.Responsible != null && interviewers.Any(i => i.Equals(x.Responsible)))
                .GroupBy(x => x.TemplateId)
                .ToDictionary(@group => new SummaryViewItem.TemplateLight(group.Key, group.FirstOrDefault().QuestionnaireTitle), @group => new SummaryViewItem.WorkingStatuses(0, 0, 0));

            foreach (var user in interviewers)
            {
                var dict = this.CloneDictionary(standard);
                this.BuildItems(
                    this.survey
                    .Query().Where(x => x.Responsible != null && x.Responsible.Equals(user))
                    .GroupBy(x => x.TemplateId),
                    dict);
                items.Add(new SummaryViewItem(user, dict));
            }

            var retval = new SummaryView(input.Page, input.PageSize, 0);
            if (input.Orders.Count > 0 && !string.IsNullOrEmpty(input.Orders[0].Field))
            {
               items = (input.Orders[0].Direction == OrderDirection.Asc
                                  ? items.OrderBy(i => this.GetOrderValue(i, input.Orders[0].Field))
                                  : items.OrderByDescending(i => this.GetOrderValue(i, input.Orders[0].Field))).ToList();
                
            }

            var summary = new Dictionary<SummaryViewItem.TemplateLight, SummaryViewItem.WorkingStatuses>(standard);
            foreach (var kvp in summary)
            {
                kvp.Value.Initial = items.Sum(i => i.Items[kvp.Key].Initial);
                kvp.Value.Redo = items.Sum(i => i.Items[kvp.Key].Redo);
                kvp.Value.Total = items.Sum(i => i.Items[kvp.Key].Total);
            }

            retval.Summary = new SummaryViewItem(
                new UserLight(Guid.Empty, "Summary"),
                summary);

            retval.TotalCount = items.Count();

            retval.Items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
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
        private object GetOrderValue(SummaryViewItem item, string field)
        {
            var parts = field.Split(new string[] { "." }, 2, StringSplitOptions.RemoveEmptyEntries);
            Guid templateId;
            if (Guid.TryParse(parts[0], out templateId))
            {
                var key = item.Items.Keys.SingleOrDefault(k => k.TemplateId == templateId);
                if (key == null)
                {
                    return 0;
                }

                switch (parts[1])
                {
                    case "Initial": return item.Items[key].Initial;
                    case "Redo": return item.Items[key].Redo;
                    case "Total": return item.Items[key].Total;
                }    
            }

            return item.User.Name;
        }

        /// <summary>
        /// New dictionary
        /// </summary>
        /// <param name="standard">
        /// The standard.
        /// </param>
        /// <returns>
        /// New cloned dictionary
        /// </returns>
        private Dictionary<SummaryViewItem.TemplateLight, SummaryViewItem.WorkingStatuses> CloneDictionary(Dictionary<SummaryViewItem.TemplateLight, SummaryViewItem.WorkingStatuses> standard)
        {
            var result = new Dictionary<SummaryViewItem.TemplateLight, SummaryViewItem.WorkingStatuses>();
            foreach (var statusese in standard)
            {
                result.Add(
                    new SummaryViewItem.TemplateLight(statusese.Key.TemplateId, statusese.Key.Title),
                    new SummaryViewItem.WorkingStatuses(0, 0, 0));
            }

            return result;
        }

        /// <summary>
        /// Builds items
        /// </summary>
        /// <param name="grouped">
        /// The grouped.
        /// </param>
        /// <param name="dictionary">
        /// The dictionary.
        /// </param>
        protected void BuildItems(IQueryable<IGrouping<Guid, CompleteQuestionnaireBrowseItem>> grouped, Dictionary<SummaryViewItem.TemplateLight, SummaryViewItem.WorkingStatuses> dictionary)
        {
            foreach (var templateGroup in grouped)
            {
                var key = new SummaryViewItem.TemplateLight(templateGroup.Key, templateGroup.FirstOrDefault().QuestionnaireTitle);
                dictionary[key] = new SummaryViewItem.WorkingStatuses(
                        templateGroup.Count(),
                        templateGroup.Count(q => q.Status.PublicId == SurveyStatus.Initial.PublicId),
                        templateGroup.Count(q => q.Status.PublicId == SurveyStatus.Redo.PublicId));
            }
        }
    }
}
