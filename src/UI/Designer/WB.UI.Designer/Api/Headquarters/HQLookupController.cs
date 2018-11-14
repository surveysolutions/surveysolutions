using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Api.Attributes;

namespace WB.UI.Designer.Api.Headquarters
{
    [ApiBasicAuth(onlyAllowedAddresses: true)]
    [RoutePrefix("api/hq/lookup")]
    public class HQLookupController : ApiController
    {
        private readonly ILookupTableService lookupTableService;
        public HQLookupController(ILookupTableService lookupTableService)
        {
            this.lookupTableService = lookupTableService;
        }

        [HttpGet]
        [Route("{id}/{tableId}")]
        public HttpResponseMessage Get(string id, string tableId)
        {
            var lookupFile = this.lookupTableService.GetLookupTableContentFile(Guid.Parse(id), Guid.Parse(tableId));

            if (lookupFile == null) return this.Request.CreateResponse(HttpStatusCode.NotFound);

            var result = JsonConvert.SerializeObject(lookupFile, Formatting.None, new JsonSerializerSettings
            { 
                TypeNameHandling = TypeNameHandling.None
            });
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(result, Encoding.UTF8, "application/json"); 
            return response;
        }
    }
}
