using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class UsersControllerBase : ApiController
    {
        private readonly IGlobalInfoProvider globalInfoProvider;
        private readonly IUserViewFactory userViewFactory;
        private readonly IUserWebViewFactory userInfoViewFactory;

        public UsersControllerBase(
            IGlobalInfoProvider globalInfoProvider,
            IUserViewFactory userViewFactory,
            IUserWebViewFactory userInfoViewFactory)
        {
            this.globalInfoProvider = globalInfoProvider;
            this.userViewFactory = userViewFactory;
            this.userInfoViewFactory = userInfoViewFactory;
        }
        
        [WriteToSyncLog(SynchronizationLogType.GetInterviewer)]
        public virtual InterviewerApiView Current()
        {
            var user = this.userViewFactory.Load(new UserViewInputModel(this.globalInfoProvider.GetCurrentUser().Id));

            return new InterviewerApiView()
            {
                Id = user.PublicKey,
                SupervisorId = user.Supervisor.Id
            };
        }
        
        [WriteToSyncLog(SynchronizationLogType.HasInterviewerDevice)]
        public virtual bool HasDevice()
        {
            var interviewerInfo = this.userInfoViewFactory.Load(new UserWebViewInputModel(this.globalInfoProvider.GetCurrentUser().Name, null));
            return !string.IsNullOrEmpty(interviewerInfo.DeviceId);
        }
    }
}