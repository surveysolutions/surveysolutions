using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class BrokenInterviewPackageApiV1Controller : ApiController
    {
        private readonly IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage;

        public BrokenInterviewPackageApiV1Controller(IPlainStorageAccessor<BrokenInterviewPackage> brokenInterviewPackageStorage)
        {
            this.brokenInterviewPackageStorage = brokenInterviewPackageStorage;
        }

        [HttpPost]
        public IHttpActionResult Post(BrokenInterviewPackageApiView package)
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
