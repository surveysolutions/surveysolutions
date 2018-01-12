using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.Code;
using WB.UI.Headquarters.Filters;

namespace WB.UI.Headquarters.Controllers
{
    public class ResourceController : BaseController
    {
        private readonly IImageFileStorage imageFileRepository;
        private readonly IPlainStorageAccessor<AudioFile> audioFileStorage;

        public ResourceController(ICommandService commandService, ILogger logger,
            IImageFileStorage imageFileRepository,
            IPlainStorageAccessor<AudioFile> audioFileStorage)
            : base(commandService, logger)
        {
            this.imageFileRepository = imageFileRepository;
            this.audioFileStorage = audioFileStorage;
        }

        [AuthorizeOr403(Roles = "Administrator, Headquarter, Supervisor")]
        public ActionResult InterviewFile(Guid interviewId, string fileName)
        {
            byte[] file = null; 
            if(fileName != null)
                file = this.imageFileRepository.GetInterviewBinaryData(interviewId, fileName);

            if (file == null || file.Length == 0)
                return
                    this.File(
                        Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("WB.UI.Headquarters.Content.img.no_image_found.jpg"),
                        "image/jpeg", "no_image_found.jpg");
            return this.File(file, "image/jpeg", fileName);
        }

        [WebInterviewAuthorize(InterviewIdQueryString = "interviewId")]
        public ActionResult AudioRecord(string interviewId, string fileName)
        {
            if (!Guid.TryParse(interviewId, out var id))
            {
                return HttpNotFound();
            }

            AudioFile file = null;
            if (fileName != null)
            {
                file = this.audioFileStorage.Query(_=> _.FirstOrDefault(x => x.InterviewId == id && x.FileName == fileName));
            }

            if (file == null || file.Data.Length == 0)
                return HttpNotFound();

            return this.File(file.Data, file.ContentType, fileName);
        }
    }
}
