using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsResolver
    {
        private readonly IMapStorageService mapStorageService;

        public MapsResolver(IMapStorageService mapStorageService)
        {
            this.mapStorageService = mapStorageService;
        }
        public IQueryable<MapBrowseItem> GetMaps() => this.mapStorageService.GetAllMaps();
        public MapBrowseItem GetMap(string id) => this.mapStorageService.GetMapById(id);
        public MapBrowseItem DeleteMap(string id) => this.mapStorageService.DeleteMap(id).Result;
        public object DeleteUserFromMap(string id, string userName) => this.mapStorageService.DeleteMapUserLink(id, userName);
        public object AddUserToMap(string id, string userName) => this.mapStorageService.AddUserToMap(id, userName);
    }
}
