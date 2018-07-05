using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class MapsApiV1Controller : MapsControllerBase
    {
        public MapsApiV1Controller(IMapStorageService mapRepository, IAuthorizedUser authorizedUser) : base(mapRepository, authorizedUser)
        {
        }

        protected override string[] GetMapsList()
        {
            return this.mapRepository.GetAllMapsForSupervisor(this.authorizedUser.Id);
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMapList)]
        public override HttpResponseMessage GetMaps() => base.GetMaps();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMap)]
        public override HttpResponseMessage GetMapContent([FromUri] string id) => base.GetMapContent(id);
    }
}
