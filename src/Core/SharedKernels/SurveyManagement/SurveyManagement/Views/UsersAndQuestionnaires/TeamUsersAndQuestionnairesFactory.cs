using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
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
                interviewSummaryReader.Query(
                    _ =>
                        _.Where(i => !i.IsDeleted && i.TeamLeadId == input.ViewerId)
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
