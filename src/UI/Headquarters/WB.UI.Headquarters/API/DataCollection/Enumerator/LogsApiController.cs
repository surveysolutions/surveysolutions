using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.API.DataCollection.Enumerator
{
    public class LogsApiController : ApiController
    {
        private readonly HqSignInManager signInManager;
        private readonly IUserViewFactory userViewFactory;
        private readonly IPlainStorageAccessor<TabletLog> logs;

        public LogsApiController(HqSignInManager signInManager, 
            IUserViewFactory userViewFactory, 
            IPlainStorageAccessor<TabletLog> logs)
        {
            this.signInManager = signInManager;
            this.userViewFactory = userViewFactory;
            this.logs = logs;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> Post()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }

            var authHeader = request.Headers.Authorization?.ToString();

            if (authHeader != null)
            {
                await signInManager.SignInWithAuthTokenAsync(authHeader, false, UserRoles.Supervisor);
            }

            var multipartMemoryStreamProvider = await request.Content.ReadAsMultipartAsync();
            var httpContent = multipartMemoryStreamProvider.Contents.Single();
            var fileContent = await httpContent.ReadAsByteArrayAsync();

            var deviceId = this.Request.Headers.GetValues(@"DeviceId").Single();
            var userId = User.Identity.GetUserId();

            var user = userId != null
                ? this.userViewFactory.GetUser(new UserViewInputModel(Guid.Parse(userId)))
                : null;

            var tabletLog = new TabletLog();
            tabletLog.Content = fileContent;
            tabletLog.DeviceId = deviceId;
            tabletLog.UserName = user?.UserName;
            tabletLog.ReceiveDateUtc = DateTime.UtcNow;
            this.logs.Store(tabletLog, null);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
