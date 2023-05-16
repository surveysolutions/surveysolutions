using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer.v2
{
    [Authorize(Roles = "Interviewer")]
    [Route("api/interviewer/v2/maps")]
    public class MapsApiV2Controller : MapsControllerBase
    {
        public MapsApiV2Controller(IMapStorageService mapRepository, IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor) 
            : base(mapRepository, authorizedUser, mapPlainStorageAccessor)
        {
        }

        [HttpGet]
        [Route("")]
        [WriteToSyncLog(SynchronizationLogType.GetMapList)]
        public override ActionResult<List<MapView>> GetMaps() => base.GetMaps();

        protected override string[] GetMapsList()
        {
            return this.mapRepository.GetAllMapsForInterviewer(this.authorizedUser.UserName);
        }

        [HttpGet]
        [Route("{id}")]
        [WriteToSyncLog(SynchronizationLogType.GetMap)]
        public override Task<IActionResult> GetMapContent([FromQuery] string id) => base.GetMapContent(id);
    }
}
