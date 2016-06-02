namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public interface ITeamUsersAndQuestionnairesFactory
    {
        TeamUsersAndQuestionnairesView Load(TeamUsersAndQuestionnairesInputModel input);
    }
}