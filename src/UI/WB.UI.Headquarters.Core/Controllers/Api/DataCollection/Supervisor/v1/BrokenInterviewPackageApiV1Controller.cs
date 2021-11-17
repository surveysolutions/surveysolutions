using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.UI.Headquarters.Controllers.Api.DataCollection.Supervisor.v1
{
    [Authorize(Roles = "Supervisor")]
    public class BrokenInterviewPackageApiV1Controller : ControllerBase
    {
        private readonly IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage;

        public BrokenInterviewPackageApiV1Controller(IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage)
        {
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
        }

        [HttpPost]
        [Route("api/supervisor/v1/brokenInterviews")]
        [RequestSizeLimit(1 * 1024 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1 * 1024 * 1024 * 1024)]
        public IActionResult Post([FromBody]BrokenInterviewPackageApiView package)
        {
            if (package == null)
                return this.BadRequest("Server cannot accept empty package content.");

            var brokenPackage = new BrokenInterviewPackage()
            {
                InterviewId = package.InterviewId,
                InterviewKey = package.InterviewKey,
                QuestionnaireId = package.QuestionnaireId,
                QuestionnaireVersion = package.QuestionnaireVersion,
                InterviewStatus = package.InterviewStatus,
                ResponsibleId = package.ResponsibleId,
                IsCensusInterview = false,
                IncomingDate = package.IncomingDate,
                Events = package.Events,
                PackageSize = package.PackageSize,
                ProcessingDate = package.ProcessingDate,
                ExceptionType = package.ExceptionType,
                ExceptionMessage = package.ExceptionMessage,
                ExceptionStackTrace = package.ExceptionStackTrace,
                ReprocessAttemptsCount = 100,
            };

            brokenInterviewPackageStorage.Store(brokenPackage, null);

            return this.Ok();
        }
    }
}
