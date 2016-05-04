using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger logger;
        private readonly IIncomingSyncPackagesQueue incomingSyncPackagesQueue;
        private readonly IPlainInterviewFileStorage plainFileRepository;

        public SyncController(IIncomingSyncPackagesQueue incomingSyncPackagesQueue, IPlainInterviewFileStorage plainFileRepository, ILogger logger)
        {
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.plainFileRepository = plainFileRepository;
            this.logger = logger;
        }

        public async Task<HttpResponseMessage> Post([FromUri]Guid interviewId)
        {
            var syncItem = await this.Request.Content.ReadAsStringAsync();

            this.incomingSyncPackagesQueue.Enqueue(interviewId: interviewId, item: syncItem);

            return Request.CreateResponse(HttpStatusCode.OK, true);
        }

        [ActionName("postfile")]
        public async Task<HttpResponseMessage> PostFile([FromUri]string fileName, [FromUri]Guid interviewId)
        {
            try
            {
                await plainFileRepository.StoreInterviewBinaryDataAsync(interviewId, fileName,
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