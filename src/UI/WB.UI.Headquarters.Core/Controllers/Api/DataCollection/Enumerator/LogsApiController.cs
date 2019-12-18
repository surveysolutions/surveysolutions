using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Enumerator
{
    public class LogsControllerBase : ControllerBase
    {
        private readonly SignInManager<HqUser> signInManager;
        private readonly IUserViewFactory userViewFactory;
        private readonly IPlainStorageAccessor<TabletLog> logs;

        public LogsControllerBase(SignInManager<HqUser> signInManager, 
            IUserViewFactory userViewFactory, 
            IPlainStorageAccessor<TabletLog> logs)
        {
            this.signInManager = signInManager;
            this.userViewFactory = userViewFactory;
            this.logs = logs;
        }

        [HttpPost]
        [Route("api/enumerator/logs")]
        public async Task<IActionResult> Post(IFormFile formFile)
        {
            var request = Request;
            if (formFile == null)
            {
                return StatusCode(StatusCodes.Status415UnsupportedMediaType);
            }

            var deviceId = this.Request.Headers["DeviceId"][0];
            var tabletLog = new TabletLog();
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);

            tabletLog.Content = memoryStream.ToArray();
            tabletLog.DeviceId = deviceId;
            tabletLog.UserName = User.FindFirstValue(ClaimTypes.Name);
            tabletLog.ReceiveDateUtc = DateTime.UtcNow;
            this.logs.Store(tabletLog, null);

            return Ok(HttpStatusCode.OK);
        }
    }
}
