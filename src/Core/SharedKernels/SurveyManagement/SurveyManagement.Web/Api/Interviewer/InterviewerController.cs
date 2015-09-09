using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Resources;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [RoutePrefix("api/interviewer/v1")]
    public class InterviewerController : ApiController
    {
        private const string RESPONSEAPPLICATIONFILENAME = "interviewer.apk";
        private const string PHYSICALAPPLICATIONFILENAME = "wbcapi.apk";
        private const string PHYSICALPATHTOAPPLICATION = "~/Client/";

        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISupportedVersionProvider versionProvider;
        private readonly ITabletInformationService tabletInformationService;
        private readonly ISyncProtocolVersionProvider syncVersionProvider;

        public InterviewerController(
            IFileSystemAccessor fileSystemAccessor,
            ISupportedVersionProvider versionProvider,
            ITabletInformationService tabletInformationService,
            ISyncProtocolVersionProvider syncVersionProvider)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.versionProvider = versionProvider;
            this.tabletInformationService = tabletInformationService;
            this.syncVersionProvider = syncVersionProvider;
        }

        [HttpGet]
        [Route("compatibility/{version:int}")]
        public HttpResponseMessage CheckInterviewerCompatibilityWithServer(int version)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();
            int supervisorShiftVersionNumber = this.syncVersionProvider.GetLastNonUpdatableVersion();

            if (version > supervisorRevisionNumber)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    ReasonPhrase = InterviewerSyncStrings.InterviewerApplicationHasHigherVersionThanSupervisor
                });
            }

            if (version < supervisorShiftVersionNumber)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    ReasonPhrase = InterviewerSyncStrings.InterviewerVersionLessThanServerOne
                });
            }

            if (version < supervisorRevisionNumber)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    ReasonPhrase = InterviewerSyncStrings.InterviewerApplicationShouldBeUpdated
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet] 
        [Route("")] 
        public HttpResponseMessage Get()
        {
            string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PHYSICALPATHTOAPPLICATION), PHYSICALAPPLICATIONFILENAME);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerApp))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, InterviewerSyncStrings.FileWasNotFound);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(this.fileSystemAccessor.ReadFile(pathToInterviewerApp))
            };

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = RESPONSEAPPLICATIONFILENAME
            };

            return response;
        }

        [HttpGet]
        [Route("latestversion")]
        public int? GetLatestVersion()
        {
            return this.versionProvider.GetApplicationBuildNumber();
        }

        [HttpPost]
        [Route("tabletInfo")]
        public HttpResponseMessage PostTabletInformation(TabletInformationPackage tabletInformationPackage)
        {
            this.tabletInformationService.SaveTabletInformation(
                content: Convert.FromBase64String(tabletInformationPackage.Content),
                androidId: tabletInformationPackage.AndroidId,
                registrationId: tabletInformationPackage.ClientRegistrationId.ToString());

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}