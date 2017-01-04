using System;
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
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    [ProtobufJsonSerializer]
    [Obsolete("Since v. 5.7")]
    public class DevicesApiV1Controller : DevicesControllerBase
    {
        public DevicesApiV1Controller(
            IGlobalInfoProvider globalInfoProvider,
            IUserWebViewFactory userInfoViewFactory,
            ISyncProtocolVersionProvider syncVersionProvider,
            ICommandService commandService,
            IReadSideRepositoryReader<TabletDocument> devicesRepository) : base(
                globalInfoProvider: globalInfoProvider,
                userInfoViewFactory: userInfoViewFactory,
                syncVersionProvider: syncVersionProvider,
                commandService: commandService,
                devicesRepository: devicesRepository)
        {
        }

        [HttpGet]
        public override HttpResponseMessage CanSynchronize(string id, int version)
        {
            return base.CanSynchronize(id, version);
        }

        [HttpPost]
        public override HttpResponseMessage LinkCurrentInterviewerToDevice(string id, int version)
        {
            return base.LinkCurrentInterviewerToDevice(id, version);
        }
    }
}