using System;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.View.User;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersUserReader : HeadquartersEntityReader, IHeadquartersUserReader
    {
        public HeadquartersUserReader(IJsonUtils jsonUtils)
            : base(jsonUtils) {}

        public async Task<UserView> GetUserByUri(Uri headquartersUserUri)
        {
            return await GetEntityByUri<UserView>(headquartersUserUri).ConfigureAwait(false);
        }
    }
}