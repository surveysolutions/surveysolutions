using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(UserRoles.Supervisor)]
    public class UpdatesApiV1Controller : ApiController
    {
        private const string PhysicalPathToApplication = "~/Client/";
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly IAndroidPackageReader androidPackageReader;
        private string pathToPatchesDirectory => HostingEnvironment.MapPath(PhysicalPathToApplication);

        public UpdatesApiV1Controller(IFileSystemAccessor fileSystemAccessor,
            IAndroidPackageReader androidPackageReader)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.androidPackageReader = androidPackageReader;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewerAppPatches)]
        public InterviewerApplicationPatchApiView[] Get()
            => this.fileSystemAccessor.GetFilesInDirectory(this.pathToPatchesDirectory, @"WBCapi.*.delta")
                .Select(x => new InterviewerApplicationPatchApiView
                {
                    FileName = this.fileSystemAccessor.GetFileName(x),
                    Url = Url.Link(@"GetInterviewerAppPatch",
                        new {id = this.fileSystemAccessor.GetFileName(x).ToLower()}),
                    SizeInBytes = this.fileSystemAccessor.GetFileSize(x)
                }).ToArray();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewerAppPatchByName)]
        public HttpResponseMessage Patch(string id)
        {
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                this.pathToPatchesDirectory, id);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);
            return new ProgressiveDownload(this.Request).ResultMessage(fileStream, @"application/octet-stream");
        }

        [HttpGet]
        public int? GetLatestVersion()
        {
            var pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(PhysicalPathToApplication), @"WBCapi.apk");

            return !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
        }
    }
}
