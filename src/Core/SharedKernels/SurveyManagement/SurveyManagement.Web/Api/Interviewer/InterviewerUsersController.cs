using System.Web.Http;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Views.SynchronizationLog;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    [ApiBasicAuth(new[] { UserRoles.Operator })]
    [RoutePrefix("api/interviewer/v1/users")]
    [ProtobufJsonSerializer]
    public class InterviewerUsersController : ApiController
    {
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserViewFactory userViewFactory;
        private readonly IUserWebViewFactory userInfoViewFactory;

        public InterviewerUsersController(
            IGlobalInfoProvider globalInfoProvider,
            IUserViewFactory userViewFactory,
            IUserWebViewFactory userInfoViewFactory)
        {
            this.globalInfoProvider = globalInfoProvider;
            this.userViewFactory = userViewFactory;
            this.userInfoViewFactory = userInfoViewFactory;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewer)]
        public InterviewerApiView Current()
        {
            var user = this.userViewFactory.Load(new UserViewInputModel(this.globalInfoProvider.GetCurrentUser().Id));

            return new InterviewerApiView()
            {
                Id = user.PublicKey,
                SupervisorId = user.Supervisor.Id
            };
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.HasInterviewerDevice)]
        public bool HasDevice()
        {
            var interviewerInfo = this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null));
            return !string.IsNullOrEmpty(interviewerInfo.DeviceId);
        }
    }
}