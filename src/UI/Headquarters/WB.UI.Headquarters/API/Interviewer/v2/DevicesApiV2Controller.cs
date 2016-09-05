using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Documents;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;


namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v2
{
    [ApiBasicAuth(new[] { UserRoles.Interviewer })]
    public class DevicesApiV2Controller : DevicesControllerBase
    {
        public DevicesApiV2Controller(
            ISyncProtocolVersionProvider syncVersionProvider,
            ICommandService commandService,
            IReadSideRepositoryReader<TabletDocument> devicesRepository,
            IIdentityManager identityManager) : base(
                identityManager: identityManager,
                syncVersionProvider: syncVersionProvider,
                commandService: commandService,
                devicesRepository: devicesRepository)
        {
        }

        [HttpGet]
        public override HttpResponseMessage CanSynchronize(string id, int version) => base.CanSynchronize(id, version);

        [HttpPost]
        public override HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version) => base.LinkCurrentInterviewerToDevice(id, version);
    }
}