using System;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersUserReader : HeadquartersEntityReader, IHeadquartersUserReader
    {
        public async Task<UserDocument> GetUserByUri(Uri headquartersUserUri)
        {
            return await GetEntityByUri<UserDocument>(headquartersUserUri).ConfigureAwait(false);
        }
    }
}