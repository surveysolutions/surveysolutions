using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.API;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Code
{
    class InterviewerVersionReader : IInterviewerVersionReader
    {
        private readonly IClientApkProvider clientApkProvider;

        public InterviewerVersionReader(IClientApkProvider clientApkProvider)
        {
            this.clientApkProvider = clientApkProvider;
        }

        public int? InterviewerBuildNumber
        {
            get
            {
                return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerFileName);
            }
        }

        public int? SupervisorBuildNumber
        {
            get
            {
                return this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.SupervisorFileName);
            }
        }

        public string InterviewerVersionString
        {
            get
            {
                return this.clientApkProvider.GetApplicationVersionString(ClientApkInfo.InterviewerFileName);
            }
        }

        public string SupervisorVersionString
        {
            get
            {
                return this.clientApkProvider.GetApplicationVersionString(ClientApkInfo.SupervisorFileName);
            }
        }
    }
}
