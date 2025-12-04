using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Storage;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [Route("Resource")]
    [ResponseCacheAttribute(NoStore = true, Location = ResponseCacheLocation.None)]
    [WebInterviewResourcesAuthorize(InterviewIdQueryString = "interviewId")]
    public class ResourceController : ControllerBase
    {
        private readonly IImageFileStorage imageFileRepository;
        private readonly IPlainStorageAccessor<AudioFile> audioFileStorage;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public ResourceController(
            IImageFileStorage imageFileRepository,
            IPlainStorageAccessor<AudioFile> audioFileStorage,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.imageFileRepository = imageFileRepository;
            this.audioFileStorage = audioFileStorage;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [Route(@"InterviewFile")]
        public async Task<ActionResult> InterviewFile(Guid interviewId, string fileName)
        {
            if (fileSystemAccessor.IsInvalidFileName(fileName))
                return BadRequest("Invalid file name");
            
            byte[] file = null; 
            if(fileName != null)
                file = await this.imageFileRepository.GetInterviewBinaryDataAsync(interviewId, fileName);

            if (file == null || file.Length == 0)
                return
                    this.File(
                        Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("WB.UI.Headquarters.Content.img.no_image_found.jpg"),
                        "image/jpeg", "no_image_found.jpg");
            
            var contentType = ContentTypeHelper.GetImageContentType(fileName);
            return this.File(file, contentType, fileName);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [Route(@"AudioRecordReview")]
        public ActionResult AudioRecordReview(string interviewId, string fileName)
        {
            if (fileSystemAccessor.IsInvalidFileName(fileName))
                return BadRequest("Invalid file name");

            if (!Guid.TryParse(interviewId, out var id))
            {
                return NotFound();
            }
            
            return GetAudioRecord(fileName, id);
        }

        [WebInterviewAuthorize(InterviewIdQueryString = "interviewId")]
        [Route(@"AudioRecord")]
        public ActionResult AudioRecord(string interviewId, string fileName)
        {
            if (fileSystemAccessor.IsInvalidFileName(fileName))
                return BadRequest("Invalid file name");
            
            if (!Guid.TryParse(interviewId, out var id))
            {
                return NotFound();
            }

            return GetAudioRecord(fileName, id);
        }

        private ActionResult GetAudioRecord(string fileName, Guid id)
        {
            AudioFile file = null;
            if (fileName != null)
            {
                file = this.audioFileStorage.Query(_ => _.FirstOrDefault(x => x.InterviewId == id && x.FileName == fileName));
            }

            if (file == null || file.Data.Length == 0)
                return NotFound();

            return this.File(file.Data, file.ContentType, fileDownloadName: fileName, enableRangeProcessing: true);
        }
    }
}
