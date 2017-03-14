using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Controllers;

namespace WB.UI.Headquarters.API
{
    [Authorize]
    [CamelCase]
    public class TroubleshootingApiController : BaseApiController
    {
        public TroubleshootingApiController(ICommandService commandService, ILogger logger) 
            : base(commandService, logger)
        {
        }

        public void FindCensusInterviews()
        {
        }
    }
}
