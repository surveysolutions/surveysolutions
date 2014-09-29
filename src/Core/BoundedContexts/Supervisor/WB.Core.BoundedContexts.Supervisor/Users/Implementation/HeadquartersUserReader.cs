using System;
using System.Threading.Tasks;
using Main.Core.View.User;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersUserReader : HeadquartersEntityReader, IHeadquartersUserReader
    {
        public HeadquartersUserReader(IJsonUtils jsonUtils, IHeadquartersSettings headquartersSettings)
            : base(jsonUtils, headquartersSettings) {}

        public async Task<UserView> GetUserByUri(Uri headquartersUserUri)
        {
            return await GetEntityByUri<UserView>(headquartersUserUri).ConfigureAwait(false);
        }
    }
}