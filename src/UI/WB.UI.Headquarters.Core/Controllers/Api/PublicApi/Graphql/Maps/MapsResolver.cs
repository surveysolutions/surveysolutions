using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.FileSystem;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Maps
{
    public class MapsResolver
    {
        private readonly IMapStorageService mapStorageService;
        private readonly IUnitOfWork unitOfWork;

        public MapsResolver(IMapStorageService mapStorageService, IUnitOfWork unitOfWork)
        {
            this.mapStorageService = mapStorageService;
            this.unitOfWork = unitOfWork;
        }

        public IQueryable<MapBrowseItem> GetMaps() => this.unitOfWork.Session.Query<MapBrowseItem>();
        public Task AddMap(IFormFile file)
        {
            var ms = new MemoryStream();
            file.CopyTo(ms);

            return this.mapStorageService.SaveOrUpdateMapAsync(new ExtractedFile
            {
                Name = file.Name,
                Size = file.Length,
                Bytes = ms.ToArray()
            });
        }

        public Task<MapBrowseItem> DeleteMap(string fileName) 
        {
            return this.mapStorageService.DeleteMap(fileName);
        }
        

        public MapBrowseItem DeleteUserFromMap(string fileName, string userName) =>
            this.mapStorageService.DeleteMapUserLink(fileName, userName);

        public MapBrowseItem AddUserToMap(string fileName, string userName) =>
            this.mapStorageService.AddUserToMap(fileName, userName);
    }
}
