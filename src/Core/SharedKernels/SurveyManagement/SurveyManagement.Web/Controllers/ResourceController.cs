﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    [Authorize(Roles = "Headquarter, Supervisor")]
    public class ResourceController : BaseController
    {
        private readonly IPlainInterviewFileStorage plainFileRepository;

        public ResourceController(ICommandService commandService, IGlobalInfoProvider provider, ILogger logger,
            IPlainInterviewFileStorage plainFileRepository)
            : base(commandService, provider, logger)
        {
            this.plainFileRepository = plainFileRepository;
        }

        public ActionResult InterviewFile(Guid interviewId, string fileName)
        {
            var file = plainFileRepository.GetInterviewBinaryData(interviewId, fileName);
            if (file == null || file.Length == 0)
                return
                    this.File(
                        Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("WB.Core.SharedKernels.SurveyManagement.Web.Content.img.no_image_found.jpg"),
                        "image/jpeg", "no_image_found.jpg");
            return this.File(file, "image/jpeg", fileName);
        }
    }
}
