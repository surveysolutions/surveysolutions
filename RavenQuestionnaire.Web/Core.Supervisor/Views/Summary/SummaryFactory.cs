namespace Core.Supervisor.Views.Summary
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
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
            var interviewers = this.GetTeamMembersForViewer(input.ViewerId).Select(x=>x.PublicKey);

            TemplateLight template = null;
            if (input.TemplateId.HasValue)
            {
                var tbi = this.templates.GetById(input.TemplateId.Value);
                template = new TemplateLight(tbi.QuestionnaireId, tbi.Title);
            }

            return this.survey.Query(queryableSurveys =>
                {
                    var items =
                        queryableSurveys.Where(
                            x =>
                            x.Responsible != null && interviewers.Contains(x.Responsible.Id)
                            && (!input.TemplateId.HasValue || x.TemplateId == input.TemplateId))
                                        .GroupBy(
                                            x => x.Responsible,
                                            x => x,
                                            (u, q) => this.BuildItems(u, q));

                        return new SummaryView(input.Page, input.PageSize, 0, template)
                                   {
                                       Summary =
                                           new SummaryViewItem(
                                           new UserLight(Guid.Empty, "Summary"),
                                           items.Sum(x => x.Total),
                                           items.Sum(x => x.Initial),
                                           items.Sum(x => x.Error),
                                           items.Sum(x => x.Completed),
                                           items.Sum(x => x.Approved),
                                           items.Sum(x => x.Redo)),
                                       TotalCount = items.Count(),
                                       Items = items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList()
                                   };
                    });
        }

        protected SummaryViewItem BuildItems(UserLight user, IEnumerable<CompleteQuestionnaireBrowseItem> questionnaires)
        {
            return new SummaryViewItem(
                user,
                questionnaires.Count(),
                questionnaires.Count(q => q.Status.PublicId == SurveyStatus.Initial.PublicId),
                questionnaires.Count(q => q.Status.PublicId == SurveyStatus.Error.PublicId),
                questionnaires.Count(q => q.Status.PublicId == SurveyStatus.Complete.PublicId),
                questionnaires.Count(q => q.Status.PublicId == SurveyStatus.Approve.PublicId),
                questionnaires.Count(q => q.Status.PublicId == SurveyStatus.Redo.PublicId));

        }
    }
}
