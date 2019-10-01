using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Shared.Web.Extensions;

namespace WB.UI.Headquarters.API.DataCollection
{
    public abstract class MapsControllerBase : ApiController
    {
        protected readonly IMapStorageService mapRepository;
        protected readonly IAuthorizedUser authorizedUser;

        protected MapsControllerBase(IMapStorageService mapRepository, IAuthorizedUser authorizedUser)
        {
            this.mapRepository = mapRepository;
            this.authorizedUser = authorizedUser;
        }

        public virtual HttpResponseMessage GetMaps()
        {
            var resultValue = GetMapsList()
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

        protected abstract string[] GetMapsList();

        public virtual async Task<HttpResponseMessage> GetMapContent([FromUri] string id)
        {
            var mapContent = await this.mapRepository.GetMapContentAsync(id);

            if (mapContent == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            return this.Request.AsProgressiveDownload(mapContent, @"application/octet-stream");
        }
    }
}
