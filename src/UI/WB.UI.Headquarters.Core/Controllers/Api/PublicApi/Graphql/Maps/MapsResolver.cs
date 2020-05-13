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
        public MapBrowseItem GetMap(string id) => this.mapStorageService.GetMapById(id);
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
        public MapBrowseItem DeleteMap(string id) => this.Execute(() => this.mapStorageService.DeleteMap(id).Result);
        public MapBrowseItem DeleteUserFromMap(string id, string userName) => this.Execute(() => this.mapStorageService.DeleteMapUserLink(id, userName));
        public MapBrowseItem AddUserToMap(string id, string userName) => this.Execute(() => this.mapStorageService.AddUserToMap(id, userName));

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
