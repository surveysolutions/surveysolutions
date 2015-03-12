using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesFactory :
        IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView>
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

            var allUsers = this.usersReader.Query(_ => _.Where(u =>
                       (u.Roles.Contains(UserRoles.Operator) && !u.IsLockedByHQ && !u.IsLockedBySupervisor && !u.IsDeleted && u.Supervisor.Id == input.ViewerId) 
                    || (u.PublicKey == input.ViewerId))
                .ToList())
                .Select(x => new UsersViewItem
                {
                    UserId = x.PublicKey,
                    UserName = x.UserName
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
