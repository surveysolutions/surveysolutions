using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Controllers.Services;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers.Api.WebInterview
{
    [Route("Resource")]
    [ResponseCacheAttribute(NoStore = true, Location = ResponseCacheLocation.None)]
    public class ResourceController : ControllerBase
    {
        private readonly IImageFileStorage imageFileRepository;
        private readonly IPlainStorageAccessor<AudioFile> audioFileStorage;

        public ResourceController(
            IImageFileStorage imageFileRepository,
            IPlainStorageAccessor<AudioFile> audioFileStorage)
        {
            this.imageFileRepository = imageFileRepository;
            this.audioFileStorage = audioFileStorage;
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [Route(@"InterviewFile")]
        public async Task<ActionResult> InterviewFile(Guid interviewId, string fileName)
        {
            byte[] file = null; 
            if(fileName != null)
                file = await this.imageFileRepository.GetInterviewBinaryDataAsync(interviewId, fileName);

            if (file == null || file.Length == 0)
                return
                    this.File(
                        Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("WB.UI.Headquarters.Content.img.no_image_found.jpg"),
                        "image/jpeg", "no_image_found.jpg");
            return this.File(file, "image/jpeg", fileName);
        }

        [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
        [Route(@"AudioRecordReview")]
        public ActionResult AudioRecordReview(string interviewId, string fileName)
        {
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
