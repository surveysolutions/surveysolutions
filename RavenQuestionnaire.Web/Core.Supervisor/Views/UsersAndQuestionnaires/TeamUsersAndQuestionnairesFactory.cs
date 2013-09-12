using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Core.Supervisor.Views.Summary;
using Core.Supervisor.Views.Survey;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesFactory :
        IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView>
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviews;

        public TeamUsersAndQuestionnairesFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviews)
        {
            this.interviews = interviews;
        }

        public TeamUsersAndQuestionnairesView Load(TeamUsersAndQuestionnairesInputModel input)
        {
            Expression<Func<InterviewSummary, bool>> predicate = (i) => !i.IsDeleted;

            predicate = predicate.AndCondition(x => x.TeamLeadId != null && x.TeamLeadId == input.ViewerId);

            var users = GetDistinctInterviews(predicate, i => new {i.ResponsibleId, i.ResponsibleName})
                .Select(x => new SurveyUsersViewItem {UserId = x.ResponsibleId, UserName = x.ResponsibleName});

            var questionnaires = GetDistinctInterviews(predicate, i => new {i.QuestionnaireId, i.QuestionnaireTitle})
                .Select(
                    x =>
                    new SummaryTemplateViewItem
                        {
                            TemplateId = x.QuestionnaireId,
                            TemplateName = x.QuestionnaireTitle
                        });

            return new TeamUsersAndQuestionnairesView
                {
                    Users = users,
                    Questionnaires = questionnaires
                };
        }

        private IList<T> GetDistinctInterviews<T>(Expression<Func<InterviewSummary, bool>> predicate,
                                          Expression<Func<InterviewSummary, T>> selector)
        {
            return this.interviews.Query(
                _ => _.Where(predicate).Select(selector).Distinct().Take(1024))
                .ToList();
        }
    }
}
