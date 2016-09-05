using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class UsersControllerBase : ApiController
    {
        private readonly IIdentityManager identityManager;
        private readonly IUserViewFactory userViewFactory;

        public UsersControllerBase(
            IIdentityManager identityManager,
            IUserViewFactory userViewFactory)
        {
            this.identityManager = identityManager;
            this.userViewFactory = userViewFactory;
        }
        
        [WriteToSyncLog(SynchronizationLogType.GetInterviewer)]
        public virtual InterviewerApiView Current()
        {
            var user = this.userViewFactory.Load(new UserViewInputModel(this.identityManager.CurrentUserId));

            return new InterviewerApiView()
            {
                Id = user.PublicKey,
                SupervisorId = user.Supervisor.Id
            };
        }
        
        [WriteToSyncLog(SynchronizationLogType.HasInterviewerDevice)]
        public virtual bool HasDevice()=> !string.IsNullOrEmpty(this.identityManager.CurrentUserDeviceId);
    }
}