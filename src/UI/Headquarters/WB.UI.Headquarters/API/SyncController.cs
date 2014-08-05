using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.Synchronization;
using WB.UI.Headquarters.API.Attributes;

namespace WB.UI.Headquarters.API
{
    [AllowAnonymous]
    [TokenValidationAuthorization]
    [HeadquarterFeatureOnly]
    public class SyncController : ApiController
    {
        private readonly ISyncManager syncManager;

        public SyncController(ISyncManager syncManager)
        {
            this.syncManager = syncManager;
        }

        public async Task<HttpResponseMessage> Post()
        {
            var syncItem = JsonConvert.DeserializeObject<SyncItem>(await this.Request.Content.ReadAsStringAsync(),
                new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.Objects
                });

            bool result = this.syncManager.SendSyncItem(syncItem);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}