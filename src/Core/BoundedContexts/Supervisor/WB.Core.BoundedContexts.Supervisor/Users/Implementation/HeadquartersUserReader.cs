using System;
using System.Net.Http;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.BoundedContexts.Supervisor.Users.Implementation
{
    internal class HeadquartersUserReader : HeadquartersEntityReader, IHeadquartersUserReader
    {
        public HeadquartersUserReader(ISerializer serializer, IHeadquartersSettings headquartersSettings, Func<HttpMessageHandler> messageHandler)
            : base(serializer, headquartersSettings, messageHandler) {}

        public async Task<UserView> GetUserByUri(Uri headquartersUserUri)
        {
            return await GetEntityByUri<UserView>(headquartersUserUri).ConfigureAwait(false);
        }
    }
}