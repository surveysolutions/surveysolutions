using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Hosting;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
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
        private string pathToPatchesDirectory => HostingEnvironment.MapPath(PhysicalPathToApplication);

        public UpdatesApiV1Controller(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetApkPatch)]
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
        [WriteToSyncLog(SynchronizationLogType.GetSupervisorApkPatch)]
        public HttpResponseMessage Patch(string id)
        {
            string pathToInterviewerPatch = this.fileSystemAccessor.CombinePath(
                this.pathToPatchesDirectory, id);

            if (!this.fileSystemAccessor.IsFileExists(pathToInterviewerPatch))
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);

            Stream fileStream = new FileStream(pathToInterviewerPatch, FileMode.Open, FileAccess.Read);
            return new ProgressiveDownload(this.Request).ResultMessage(fileStream, @"application/octet-stream");
        }
    }
}
