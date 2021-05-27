using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;
using WB.Infrastructure.Native.Sanitizer;
using WB.UI.Shared.Web.Controllers;

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
        public async Task<IActionResult> Post()
        {
            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                new FormOptions().MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary.ToString(), HttpContext.Request.Body);

            var section = await reader.ReadNextSectionAsync();

            if (section != null)
            {
                var formData = new MemoryStream();
                await  section.Body.CopyToAsync(formData);

                var deviceId = this.Request.Headers["DeviceId"].Single().RemoveHtmlTags();

                var tabletLog = new TabletLog();
                
                tabletLog.Content = formData.ToArray();
                tabletLog.DeviceId = deviceId;
                tabletLog.UserName = User.FindFirstValue(ClaimTypes.Name);
                tabletLog.ReceiveDateUtc = DateTime.UtcNow;
                this.logs.Store(tabletLog, null);
                
            }
            
            return Ok(HttpStatusCode.OK);
        }
    }
}
