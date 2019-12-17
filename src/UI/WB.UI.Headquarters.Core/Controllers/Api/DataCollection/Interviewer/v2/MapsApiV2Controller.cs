using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    [Route("api/interviewer/v2/maps")]
    public class MapsApiV2Controller : MapsControllerBase
    {
        public MapsApiV2Controller(IMapStorageService mapRepository, IAuthorizedUser authorizedUser) : base(mapRepository, authorizedUser)
        {
        }

        [HttpGet]
        [Route("")]
        public override ActionResult<List<MapView>> GetMaps() => base.GetMaps();

        protected override string[] GetMapsList()
        {
            return this.mapRepository.GetAllMapsForInterviewer(this.authorizedUser.UserName);
        }

        [HttpGet]
        [Route("{id}")]
        public override Task<IActionResult> GetMapContent([FromQuery] string id) => base.GetMapContent(id);
    }
}
