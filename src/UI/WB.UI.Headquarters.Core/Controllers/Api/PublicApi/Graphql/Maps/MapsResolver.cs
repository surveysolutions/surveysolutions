#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsResolver
    {
        public IQueryable<MapBrowseItem> GetMaps([Service] IUnitOfWork unitOfWork) => 
            unitOfWork.Session.Query<MapBrowseItem>();
        
        public Task<MapBrowseItem> DeleteMap(string fileName, [Service] IMapStorageService mapStorageService) 
        {
            return mapStorageService.DeleteMap(fileName);
        }
        

        public MapBrowseItem DeleteUserFromMap(string fileName, string userName,[Service] IMapStorageService mapStorageService) =>
            mapStorageService.DeleteMapUserLink(fileName, userName);

        public MapBrowseItem AddUserToMap(string fileName, string userName,[Service] IMapStorageService mapStorageService) =>
            mapStorageService.AddUserToMap(fileName, userName);
    }
}
