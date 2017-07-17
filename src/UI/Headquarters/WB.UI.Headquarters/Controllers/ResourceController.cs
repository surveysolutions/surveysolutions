using System;
using System.Reflection;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter, Supervisor")]
    public class ResourceController : BaseController
    {
        private readonly IFileSystemInterviewFileStorage fileSystemFileRepository;

        public ResourceController(ICommandService commandService, ILogger logger,
            IFileSystemInterviewFileStorage fileSystemFileRepository)
            : base(commandService, logger)
        {
            this.fileSystemFileRepository = fileSystemFileRepository;
        }

        public ActionResult InterviewFile(Guid interviewId, string fileName)
        {
            byte[] file = null; 
            if(fileName != null)
                file = this.fileSystemFileRepository.GetInterviewBinaryData(interviewId, fileName);

            if (file == null || file.Length == 0)
                return
                    this.File(
                        Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("WB.UI.Headquarters.Content.img.no_image_found.jpg"),
                        "image/jpeg", "no_image_found.jpg");
            return this.File(file, "image/jpeg", fileName);
        }
    }
}
