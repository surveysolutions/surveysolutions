using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class MapsApiV2Controller : ApiController
    {
        private readonly IMapStorageService mapRepository;
        private readonly IAuthorizedUser authorizedUser;

        public MapsApiV2Controller(IMapStorageService mapRepository, IAuthorizedUser authorizedUser)
        {
            this.mapRepository = mapRepository;
            this.authorizedUser = authorizedUser;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMapList)]
        public virtual HttpResponseMessage GetMaps()
        {
            var resultValue = this.mapRepository.GetAllMapsForUser(this.authorizedUser.UserName)
                .Select(x => new MapView(){MapName = x})
                .ToList();

            var response = this.Request.CreateResponse(resultValue);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = false,
                NoCache = true
            };

            return response;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMap)]
        public HttpResponseMessage GetMapContent([FromUri]string id)
        {
            var mapContent = this.mapRepository.GetMapContent(id);

            if (mapContent == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(mapContent)
            };

            response.Headers.CacheControl = new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(10)
            };

            return response;
        }
    }
}
