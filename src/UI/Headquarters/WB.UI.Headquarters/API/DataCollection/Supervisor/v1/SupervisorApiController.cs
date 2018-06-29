using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Microsoft.AspNet.Identity;
using WB.Core.BoundedContexts.Headquarters.OwinSecurity;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.Code;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    public class SupervisorAppApiController : ApiController
    {
        private const string RESPONSEAPPLICATIONFILENAME = "supervisor.apk";
        private const string PHYSICALAPPLICATIONFILENAME = "supervisor.apk";
        private const string PHYSICALPATHTOAPPLICATION = "~/Client/";
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAndroidPackageReader androidPackageReader;

        private readonly HqSignInManager signInManager;
        protected readonly IUserViewFactory userViewFactory;
        protected readonly ITabletInformationService tabletInformationService;

        public SupervisorAppApiController(
            IFileSystemAccessor fileSystemAccessor,
            IAndroidPackageReader androidPackageReader, 
            ITabletInformationService tabletInformationService, 
            IUserViewFactory userViewFactory, 
            HqSignInManager signInManager)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.androidPackageReader = androidPackageReader;
            this.tabletInformationService = tabletInformationService;
            this.userViewFactory = userViewFactory;
            this.signInManager = signInManager;
        }

        [HttpGet]
        public virtual int? GetLatestVersion()
        {
            string pathToInterviewerApp =
                this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION),
                    PHYSICALAPPLICATIONFILENAME);

            return !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
        }

        [ApiBasicAuth(UserRoles.Supervisor)]
        [HttpGet]
        [ApiNoCache]
        public virtual HttpResponseMessage CheckCompatibility(string deviceId, int deviceSyncProtocolVersion)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, @"158329303");
        }

        private Version GetSupervisorVersionFromUserAgent(HttpRequestMessage request)
        {
            foreach (var product in request.Headers?.UserAgent)
            {
                if ((product.Product?.Name.Equals(@"org.worldbank.solutions.supervisor",
                         StringComparison.OrdinalIgnoreCase) ?? false)
                    && Version.TryParse(product.Product.Version, out Version version))
                {
                    return version;
                }
            }

            return null;
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostTabletInformation()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
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

            this.tabletInformationService.SaveTabletInformation(
                content: fileContent,
                androidId: deviceId,
                user: user);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
