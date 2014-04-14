using System;
using System.Threading.Tasks;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Supervisor.Users
{
    public interface IHeadquartersUserReader
    {
        Task<UserDocument> GetUserByUri(Uri headquartersUserUri);
    }
}