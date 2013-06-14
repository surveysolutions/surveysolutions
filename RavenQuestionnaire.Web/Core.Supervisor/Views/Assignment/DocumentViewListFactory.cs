using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Views.Assignment
{
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.Core.View.Questionnaire;

    using Main.DenormalizerStorage;

    public class DocumentViewListFactory : BaseUserViewFactory, IViewFactory<AssignmentInputModel, AssignmentView>
    {
        private readonly IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem> surveys;

        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> templates;

        public DocumentViewListFactory(
            IQueryableReadSideRepositoryReader<CompleteQuestionnaireBrowseItem> surveys,
            IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> templates,
            IQueryableReadSideRepositoryReader<UserDocument> users)
            : base(users)
        {
            this.surveys = surveys;
            this.templates = templates;
        }

        public AssignmentView Load(AssignmentInputModel input)
        {
            return this.surveys.Query(queryableSurveys =>
            {
                var responsibleList = GetTeamMembersForViewer(input.ViewerId).Select(i => i.PublicKey);
                var view = new AssignmentView(input.Page, input.PageSize, 0);

                view.AssignableUsers = this.IsHq(input.ViewerId)
                    ? this.GetSupervisorsListForViewer(input.ViewerId)
                    : this.GetInterviewersListForViewer(input.ViewerId);

                view.Template = !input.TemplateId.HasValue
                                ? null
                                : this.templates.GetById(input.TemplateId.Value).GetTemplateLight();

                view.User = !input.InterviewerId.HasValue
                                ? null
                                : this.users.GetById(input.InterviewerId.Value).GetUseLight();

                view.Status = SurveyStatus.Unknown;

                if (input.StatusId.HasValue)
                {
                    view.Status = SurveyStatus.GetStatusByIdOrDefault(input.StatusId);
                }
                #warning need to be filtered by responsible supervisr
                #warning ReadLayer: Contains is not supported in Raven, but In should be used, but here we are abstracted and cannot use it, so more data is now processed on client-side
                IQueryable<CompleteQuestionnaireBrowseItem> items = (view.Status.PublicId == SurveyStatus.Unknown.PublicId
                                                                         ? queryableSurveys
                                                                         : queryableSurveys
                                                                               .Where(
                                                                                   v =>
                                                                                   v.Status.PublicId == view.Status.PublicId))
                    .ToList()
                    .AsQueryable()
                    .Where(q => (q.Responsible == null) || responsibleList.Contains(q.Responsible.Id))
                    .OrderByDescending(t => t.CreationDate);

                if (input.TemplateId.HasValue)
                {
                    items = items.Where(x => (x.TemplateId == input.TemplateId));
                }

                if (input.IsNotAssigned)
                {
                    items = items.Where(t => t.Responsible == null);
                }
                else if (input.InterviewerId.HasValue)
                {
                    items = items.Where(t => t.Responsible != null).Where(x => x.Responsible.Id == input.InterviewerId);
                }

                if (input.QuestionnaireId.HasValue)
                {
                    items = items.Where(t => t.CompleteQuestionnaireId == input.QuestionnaireId);
                }

                view.TotalCount = items.Count();

                if (input.Orders.Count > 0)
                {
                    items = this.DefineOrderBy(items, input);
                }

                view.SetItems(items.Skip((input.Page - 1) * input.PageSize).Take(input.PageSize).ToList());


                return view;
            });
        }

        private IQueryable<CompleteQuestionnaireBrowseItem> DefineOrderBy(
            IQueryable<CompleteQuestionnaireBrowseItem> query, AssignmentInputModel input)
        {
            List<string> o = query.SelectMany(t => t.FeaturedQuestions).Select(y => y.Title).Distinct().ToList();
            if (o.Contains(input.Orders[0].Field))
            {
                query = input.Orders[0].Direction == OrderDirection.Asc
                            ? query.OrderBy(
                                t =>
                                t.FeaturedQuestions.Where(y => y.Title == input.Orders[0].Field).Select(
                                    x => x.Answer.ToString()).FirstOrDefault())
                            : query.OrderByDescending(
                                t =>
                                t.FeaturedQuestions.Where(y => y.Title == input.Orders[0].Field).Select(
                                    x => x.Answer.ToString()).FirstOrDefault());
            }
            else
            {
                if (input.Orders[0].Field.Contains("Responsible"))
                {
                    IQueryable<CompleteQuestionnaireBrowseItem> usersNull = query.Where(t => t.Responsible == null);
                    IOrderedQueryable<CompleteQuestionnaireBrowseItem> contains = input.Orders[0].Direction
                                                                                  == OrderDirection.Asc
                                                                                      ? query.Where(
                                                                                          t => t.Responsible != null).
                                                                                            OrderBy(
                                                                                                input.Orders[0].Field)
                                                                                      : query.Where(
                                                                                          t => t.Responsible != null).
                                                                                            OrderByDescending(
                                                                                                input.Orders[0].Field);
                    query = (input.Orders[0].Direction == OrderDirection.Asc)
                                ? usersNull.Union(contains)
                                : contains.Union(usersNull);
                }
                else
                {
                    query = input.Orders[0].Direction == OrderDirection.Asc
                                ? query.OrderBy(input.Orders[0].Field)
                                : query.OrderByDescending(input.Orders[0].Field);
                }
            }

            return query;
        }
    }
}