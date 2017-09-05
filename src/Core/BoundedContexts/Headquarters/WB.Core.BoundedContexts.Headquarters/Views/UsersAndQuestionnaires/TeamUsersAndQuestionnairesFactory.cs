using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesFactory : ITeamUsersAndQuestionnairesFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader;
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader;
        public TeamUsersAndQuestionnairesFactory(
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnairesReader, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryReader)
        {
            this.questionnairesReader = questionnairesReader;
            this.interviewSummaryReader = interviewSummaryReader;
        }

        public TeamUsersAndQuestionnairesView Load(TeamUsersAndQuestionnairesInputModel input)
        {
            var allUsers =
                this.interviewSummaryReader.Query(
                    _ =>
                        _.Where(i => i.TeamLeadId == input.ViewerId)
                            .GroupBy(x => new {x.ResponsibleId, x.ResponsibleName})
                            .Where(x => x.Count() > 0)
                            .Select(x => new UsersViewItem {UserId = x.Key.ResponsibleId, UserName = x.Key.ResponsibleName})
                            .OrderBy(x => x.UserName).ToList());


            List<QuestionnaireBrowseItem> allQuestionnaires = this.questionnairesReader.Query(x => x.Where(q => !q.IsDeleted).ToList());

            var questionnaires = allQuestionnaires.Select(questionnaire => new TemplateViewItem
            {
                TemplateId = questionnaire.QuestionnaireId,
                TemplateName = questionnaire.Title,
                TemplateVersion = questionnaire.Version
            }).OrderBy(x => x.TemplateName).ThenBy(n => n.TemplateVersion).ToList();

            return new TeamUsersAndQuestionnairesView
                {
                    Users = allUsers,
                    Questionnaires = questionnaires
                };
        }
    }
}
