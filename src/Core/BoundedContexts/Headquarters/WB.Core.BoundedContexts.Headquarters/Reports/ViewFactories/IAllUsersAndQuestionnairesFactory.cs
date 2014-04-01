using WB.Core.BoundedContexts.Headquarters.Reports.Views;

namespace WB.Core.BoundedContexts.Headquarters.Reports.ViewFactories
{
    public interface IAllUsersAndQuestionnairesFactory 
    {
        AllUsersAndQuestionnairesView Load();
    }
}