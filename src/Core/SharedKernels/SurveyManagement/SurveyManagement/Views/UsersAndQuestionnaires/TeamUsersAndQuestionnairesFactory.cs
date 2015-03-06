using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesFactory :
        IViewFactory<TeamUsersAndQuestionnairesInputModel, TeamUsersAndQuestionnairesView>
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public TeamUsersAndQuestionnairesFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public TeamUsersAndQuestionnairesView Load(TeamUsersAndQuestionnairesInputModel input)
        {
            string questionnaireIndexName = typeof(QuestionnaireBrowseItemsGroupByQuestionnaireIdIndex).Name;
            string userIndexName = typeof(UserDocumentsByBriefFields).Name;

            var allUsers = indexAccessor.Query<UserDocument>(userIndexName)
                .Where(
                    u =>
                        (u.Roles.Contains(UserRoles.Operator) && !u.IsLockedByHQ && !u.IsLockedBySupervisor &&
                         !u.IsDeleted && u.Supervisor.Id == input.ViewerId) ||
                        (u.PublicKey == input.ViewerId))
                .QueryAll()
                .Select(x => new UsersViewItem {UserId = x.PublicKey, UserName = x.UserName});

            var allQuestionnaires = indexAccessor.Query<QuestionnaireAndVersionsItem>(questionnaireIndexName).QueryAll();

            var questionnaires = allQuestionnaires.SelectMany(questionnaire => questionnaire.Versions.Select(version => new TemplateViewItem
            {
                TemplateId = questionnaire.QuestionnaireId,
                TemplateName = questionnaire.Title,
                TemplateVersion = version
            })).ToList();

            return new TeamUsersAndQuestionnairesView
                {
                    Users = allUsers,
                    Questionnaires = questionnaires
                };
        }
    }
}
