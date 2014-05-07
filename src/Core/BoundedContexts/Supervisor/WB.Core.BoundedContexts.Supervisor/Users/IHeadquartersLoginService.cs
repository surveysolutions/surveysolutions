using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Supervisor.Users
{
    public interface IHeadquartersLoginService
    {
        Task LoginAndCreateAccount(string login, string password);
    }
}