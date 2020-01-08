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

        public int? Version
        {
            get
            {
                return this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerFileName);
            }
        }

        public int? SupervisorVersion
        {
            get
            {
                return this.clientApkProvider.GetLatestVersion(ClientApkInfo.SupervisorFileName);
            }
        }
    }
}
