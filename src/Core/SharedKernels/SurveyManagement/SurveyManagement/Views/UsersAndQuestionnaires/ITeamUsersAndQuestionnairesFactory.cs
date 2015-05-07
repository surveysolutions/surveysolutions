namespace WB.Core.SharedKernels.SurveyManagement.Views.UsersAndQuestionnaires
{
    public interface ITeamUsersAndQuestionnairesFactory
    {
        TeamUsersAndQuestionnairesView Load(TeamUsersAndQuestionnairesInputModel input);
    }
}