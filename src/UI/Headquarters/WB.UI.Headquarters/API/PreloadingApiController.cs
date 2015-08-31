using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;

namespace WB.UI.Headquarters.API
{
    public class PreloadingApiController : ApiController
    {
        private readonly IUserPreloadingService userPreloadingService;

        public PreloadingApiController(IUserPreloadingService userPreloadingService)
        {
            this.userPreloadingService = userPreloadingService;
        }

        public UserPreloadingProcess UserPreloadingDetails(string id)
        {
            return userPreloadingService.GetPreloadingProcesseDetails(id);
        }
    }
}