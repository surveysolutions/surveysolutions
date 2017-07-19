using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
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

        public ActionResult AudioRecord(Guid interviewId, string fileName)
        {
            AudioFile file = null;
            if (fileName != null)
            {
                file = this.audioFileStorage.Query(_=> _.FirstOrDefault(x => x.InterviewId == interviewId && x.FileName == fileName));
            }

            if (file == null || file.Data.Length == 0)
                return HttpNotFound();

            return this.File(file.Data, file.ContentType, fileName);
        }
    }
}
