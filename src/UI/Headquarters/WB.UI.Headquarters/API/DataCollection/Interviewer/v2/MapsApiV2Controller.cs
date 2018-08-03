using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class MapsApiV2Controller : MapsControllerBase
    {
        public MapsApiV2Controller(IMapStorageService mapRepository, IAuthorizedUser authorizedUser) : base(mapRepository, authorizedUser)
        {
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMapList)]
        public override HttpResponseMessage GetMaps() => base.GetMaps();

        protected override string[] GetMapsList()
        {
            return this.mapRepository.GetAllMapsForInterviewer(this.authorizedUser.UserName);
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetMap)]
        public override HttpResponseMessage GetMapContent([FromUri] string id) => base.GetMapContent(id);
    }
}
