using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class MapsControllerBase : ControllerBase
    {
        protected readonly IMapStorageService mapRepository;
        protected readonly IAuthorizedUser authorizedUser;
        private readonly IUserRepository userRepository;
        private readonly IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor;

        protected MapsControllerBase(IMapStorageService mapRepository, IAuthorizedUser authorizedUser, 
            IPlainStorageAccessor<MapBrowseItem> mapPlainStorageAccessor,
            IUserRepository userRepository)
        {
            this.mapRepository = mapRepository;
            this.authorizedUser = authorizedUser;
            this.mapPlainStorageAccessor = mapPlainStorageAccessor;
            this.userRepository = userRepository;
        }

        public virtual ActionResult<List<MapView>> GetMaps()
        {
            List<MapView> resultValue = GetMapsList()
                .Select(x => new MapView { MapName = x })
                .ToList();

            return resultValue;
        }

        protected abstract string[] GetMapsList();

        public virtual async Task<IActionResult> GetMapContent(string id)
        {
            MapBrowseItem map = await mapPlainStorageAccessor.GetByIdAsync(id);
            if (map == null)
                return NotFound();
            
            if (map.Users.All(u => u.UserName != authorizedUser.UserName))
            {
                if (authorizedUser.IsSupervisor)
                {
                    var team =
                        userRepository.Users.Where(user =>
                                user.WorkspaceProfile.SupervisorId == authorizedUser.Id || user.Id == authorizedUser.Id)
                            .Select(x => x.UserName).ToArray();
                
                    if(!map.Users.Any(u => team.Contains(u.UserName)))
                        return Forbid(); 
                }
                else
                    return Forbid();
            }

            var mapContent = await this.mapRepository.GetMapContentAsync(id);
            if (mapContent == null)
                return NotFound();

            Stream exportFileStream = new MemoryStream(mapContent);
            var result = new FileStreamResult(exportFileStream, "application/octet-stream") { };
            return result;
        }
    }
}
