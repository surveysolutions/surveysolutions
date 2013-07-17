using System;
using System.Linq.Expressions;
using Raven.Client.Linq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire.BrowseItem;

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
            
            Expression<Func<CompleteQuestionnaireBrowseItem, bool>> predicate = (s) =>
                                                                                s.Responsible == null ||
                                                                                s.Responsible.Id.In<Guid>(
                                                                                    responsibleList);

            if (view.Status.PublicId != SurveyStatus.Unknown.PublicId)
            {
                predicate = AndCondition(predicate, s => s.Status.PublicId == view.Status.PublicId);
            }

            if (input.TemplateId.HasValue)
            {
                predicate = AndCondition(predicate, s => s.TemplateId == input.TemplateId);
            }

            if (input.IsNotAssigned)
            {
                predicate = AndCondition(predicate, s => s.Responsible == null);
            }
            else if (input.InterviewerId.HasValue)
            {
                predicate = AndCondition(predicate, s => s.Responsible != null && s.Responsible.Id == input.InterviewerId);
            }

            if (input.QuestionnaireId.HasValue)
            {
                predicate = AndCondition(predicate, s => s.CompleteQuestionnaireId == input.QuestionnaireId);
            }

            IQueryable<CompleteQuestionnaireBrowseItem> items =
                this.surveys.QueryEnumerable(predicate).Skip((input.Page - 1) * input.PageSize).Take(input.PageSize)
                    .OrderByDescending(t => t.CreationDate).ToList().AsQueryable();


            view.TotalCount = this.surveys.Count(predicate);

#warning this order by is wrong. It is doing sorting awith paged results
            if (input.Orders.Count > 0)
            {
                items = this.DefineOrderBy(items, input);
            }

            view.SetItems(items.ToList());


            return view;

        }

        private Expression<Func<CompleteQuestionnaireBrowseItem, bool>> AndCondition(Expression<Func<CompleteQuestionnaireBrowseItem, bool>> predicate, Expression<Func<CompleteQuestionnaireBrowseItem, bool>> condition)
        {
          //  return predicate.AndAlso(condition);
            return Expression.Lambda<Func<CompleteQuestionnaireBrowseItem, bool>>(
                Expression.AndAlso(predicate.Body, condition.Body), predicate.Parameters);
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