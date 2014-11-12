using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
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

            var users = this.GetDistinctInterviews(predicate, i => new {i.ResponsibleId, i.ResponsibleName})
                .Select(x => new UsersViewItem {UserId = x.ResponsibleId, UserName = x.ResponsibleName});

            var questionnaires = this.GetDistinctInterviews(predicate, i => new { i.QuestionnaireId, i.QuestionnaireTitle, i.QuestionnaireVersion })
                .Select(
                    x =>
                    new TemplateViewItem
                        {
                            TemplateId = x.QuestionnaireId,
                            TemplateName = x.QuestionnaireTitle,
                            TemplateVersion = x.QuestionnaireVersion
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
