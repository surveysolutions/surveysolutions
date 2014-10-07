using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.Synchronization;
using WB.UI.Headquarters.API.Attributes;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API
{
    [AllowAnonymous]
    [TokenValidationAuthorization]
    [HeadquarterFeatureOnly]
    public class SyncController : ApiController
    {
        private readonly ISyncManager syncManager;
        private readonly ILogger logger;
        private readonly IPlainInterviewFileStorage plainFileRepository;

        public SyncController(ISyncManager syncManager, IPlainInterviewFileStorage plainFileRepository, ILogger logger)
        {
            this.syncManager = syncManager;
            this.plainFileRepository = plainFileRepository;
            this.logger = logger;
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

        [ActionName("postfile")]
        public async Task<HttpResponseMessage> PostFile([FromUri]string fileName, [FromUri]Guid interviewId)
        {
            try
            {
                plainFileRepository.StoreInterviewBinaryData(interviewId, fileName,
                    await this.Request.Content.ReadAsByteArrayAsync());

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }
    }
}