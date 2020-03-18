using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.API;

namespace WB.UI.Headquarters.Services.Impl
{
    class InterviewerVersionReader : IInterviewerVersionReader
    {
        private readonly IClientApkProvider clientApkProvider;

        public InterviewerVersionReader(IClientApkProvider clientApkProvider)
        {
            this.clientApkProvider = clientApkProvider;
        }

        public int? InterviewerBuildNumber => this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerFileName);

        public int? SupervisorBuildNumber => this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.SupervisorFileName);

        public string InterviewerVersionString => this.clientApkProvider.GetApplicationVersionString(ClientApkInfo.InterviewerFileName);

        public string SupervisorVersionString => this.clientApkProvider.GetApplicationVersionString(ClientApkInfo.SupervisorFileName);
    }
}
