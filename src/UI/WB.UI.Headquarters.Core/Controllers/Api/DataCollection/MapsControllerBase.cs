using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection
{
    public abstract class MapsControllerBase : ControllerBase
    {
        protected readonly IMapStorageService mapRepository;
        protected readonly IAuthorizedUser authorizedUser;

        protected MapsControllerBase(IMapStorageService mapRepository, IAuthorizedUser authorizedUser)
        {
            this.mapRepository = mapRepository;
            this.authorizedUser = authorizedUser;
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
            var mapContent = await this.mapRepository.GetMapContentAsync(id);
            if (mapContent == null)
                return NotFound();

            Stream exportFileStream = new MemoryStream(mapContent);
            var result = new FileStreamResult(exportFileStream, "application/octet-stream") { };
            return result;
        }
    }
}
