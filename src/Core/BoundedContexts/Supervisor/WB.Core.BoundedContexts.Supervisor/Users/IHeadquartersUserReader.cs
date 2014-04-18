using System;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.View.User;

namespace WB.Core.BoundedContexts.Supervisor.Users
{
    public interface IHeadquartersUserReader
    {
        Task<UserView> GetUserByUri(Uri headquartersUserUri);
    }
}