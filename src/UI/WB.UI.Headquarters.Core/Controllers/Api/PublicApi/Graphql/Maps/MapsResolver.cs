using System;
using System.IO;
using System.Linq;
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
        public IQueryable<MapBrowseItem> GetMaps() => this.mapStorageService.GetAllMaps();
        public MapBrowseItem GetMap(string fileName) => this.mapStorageService.GetMapById(fileName);
        public MapBrowseItem AddMap(IFormFile file)
        {
            var ms = new MemoryStream();
            file.CopyTo(ms);
            
            return this.Execute(() => this.mapStorageService.SaveOrUpdateMapAsync(new ExtractedFile
            {
                Name = file.Name,
                Size = file.Length,
                Bytes = ms.ToArray()
            }).Result);
        }
        public MapBrowseItem DeleteMap(string fileName) => this.Execute(() => this.mapStorageService.DeleteMap(fileName).Result);
        public MapBrowseItem DeleteUserFromMap(string fileName, string userName) => this.Execute(() => this.mapStorageService.DeleteMapUserLink(fileName, userName));
        public MapBrowseItem AddUserToMap(string fileName, string userName) => this.Execute(() => this.mapStorageService.AddUserToMap(fileName, userName));

        private T Execute<T>(Func<T> action)
        {
            T result = default;
            
            try
            {
                result = action();
                this.unitOfWork.AcceptChanges();
            }
            catch
            {
                this.unitOfWork.DiscardChanges();
            }

            return result;
        }
    }
}
