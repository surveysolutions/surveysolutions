using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesFactory : ITeamUsersAndQuestionnairesFactory
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> usersReader;
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesReader;

        public TeamUsersAndQuestionnairesFactory(IQueryableReadSideRepositoryReader<UserDocument> usersReader,
            IQueryableReadSideRepositoryReader<QuestionnaireBrowseItem> questionnairesReader)
        {
            this.usersReader = usersReader;
            this.questionnairesReader = questionnairesReader;
        }

        public TeamUsersAndQuestionnairesView Load(TeamUsersAndQuestionnairesInputModel input)
        {
            var allUsers = this.usersReader.Query(_ =>
            {
                var users = (from u in _
                    where
                        (u.Roles.Contains(UserRoles.Operator) && !u.IsArchived && !u.IsLockedByHQ && !u.IsLockedBySupervisor && u.Supervisor.Id == input.ViewerId)
                     || (u.PublicKey == input.ViewerId)
                    select new UsersViewItem
                    {
                        UserId = u.PublicKey,
                        UserName = u.UserName
                    }).ToList();
                return users;
            });


            List<QuestionnaireBrowseItem> allQuestionnaires = this.questionnairesReader.Query(_ => _.ToList());

            var questionnaires = allQuestionnaires.Select(questionnaire => new TemplateViewItem
            {
                TemplateId = questionnaire.QuestionnaireId,
                TemplateName = questionnaire.Title,
                TemplateVersion = questionnaire.Version
            }).ToList();

            return new TeamUsersAndQuestionnairesView
                {
                    Users = allUsers,
                    Questionnaires = questionnaires
                };
        }
    }
}
