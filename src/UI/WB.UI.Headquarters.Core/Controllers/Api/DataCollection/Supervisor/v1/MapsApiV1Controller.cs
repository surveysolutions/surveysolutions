using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    [Route("api/supervisor/v1/maps")]
    public class MapsApiV1Controller : MapsControllerBase
    {
        public MapsApiV1Controller(IMapStorageService mapRepository, IAuthorizedUser authorizedUser,
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor, IUserRepository userRepository) 
            : base(mapRepository, authorizedUser, mapPlainStorageAccessor, userRepository)
        {
        }

        protected override string[] GetMapsList()
        {
            return this.mapRepository.GetAllMapsForSupervisor(this.authorizedUser.Id);
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMapList)]
        [Route("")]
        public override ActionResult<List<MapView>> GetMaps() => base.GetMaps();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMap)]
        [Route("{id}")]
        public override Task<IActionResult> GetMapContent([FromQuery] string id) => base.GetMapContent(id);
    }
}
