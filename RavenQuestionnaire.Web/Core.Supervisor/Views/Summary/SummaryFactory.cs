namespace Core.Supervisor.Views.Summary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.Questionnaire;
    using Main.DenormalizerStorage;

    public class SummaryFactory : BaseUserViewFactory, IViewFactory<SummaryInputModel, SummaryView>
    {
        private readonly IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> survey;

        private readonly IQueryableDenormalizerStorage<QuestionnaireBrowseItem> templates;

        public SummaryFactory(
            IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem> survey,
            IQueryableDenormalizerStorage<QuestionnaireBrowseItem> templates,
            IQueryableDenormalizerStorage<UserDocument> users) : base(users)
        {
            this.survey = survey;
            this.templates = templates;
        }
        
        public SummaryView Load(SummaryInputModel input)
        {
            var interviewers = this.GetTeamMembersForViewer(input.ViewerId).Select(u => u.PublicKey).ToList();
            TemplateLight template = null;
            if (input.TemplateId.HasValue)
            {
                var tbi = this.templates.GetById(input.TemplateId.Value);
                template = new TemplateLight(tbi.Id, tbi.Title);
            }

            var items = this.BuildItems((!input.TemplateId.HasValue
                                             ? this.survey.Query().Where(x => x.Responsible != null && interviewers.Contains(x.Responsible.Id))
                                             : this.survey.Query().Where(
                                                 x => x.Responsible != null && interviewers.Contains(x.Responsible.Id) && (x.TemplateId == input.TemplateId)))
                .GroupBy(x => x.Responsible))
                .AsQueryable();

            var retval = new SummaryView(input.Page, input.PageSize, 0, template);
            if (input.Orders.Count > 0)
            {
                items = input.Orders[0].Direction == OrderDirection.Asc
                                                      ? items.OrderBy(input.Orders[0].Field)
                                                      : items.OrderByDescending(input.Orders[0].Field);
            }

            retval.Summary = new SummaryViewItem(
                new UserLight(Guid.Empty, "Summary"),
                items.Sum(x => x.Total),
                items.Sum(x => x.Initial),
                items.Sum(x => x.Error),
                items.Sum(x => x.Completed),
                items.Sum(x => x.Approved),
                items.Sum(x => x.Redo));

            retval.TotalCount = items.Count();

            retval.Items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList();
            return retval;
        }

        protected IEnumerable<SummaryViewItem> BuildItems(IQueryable<IGrouping<UserLight, CompleteQuestionnaireBrowseItem>> grouped)
        {
            foreach (var templateGroup in grouped)
            {
                yield
                    return new SummaryViewItem(templateGroup.Key,
                                            templateGroup.Count(),
                                            templateGroup.Count(q => q.Status.PublicId == SurveyStatus.Initial.PublicId),
                                            templateGroup.Count(q => q.Status.PublicId == SurveyStatus.Error.PublicId),
                                            templateGroup.Count(q => q.Status.PublicId == SurveyStatus.Complete.PublicId),
                                            templateGroup.Count(q => q.Status.PublicId == SurveyStatus.Approve.PublicId), templateGroup.Count(q => q.Status.PublicId == SurveyStatus.Redo.PublicId));
            }
        }
    }
}
