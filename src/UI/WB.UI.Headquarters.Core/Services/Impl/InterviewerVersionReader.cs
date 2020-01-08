using WB.Core.BoundedContexts.Headquarters.Services;
using WB.UI.Headquarters.API;

namespace WB.UI.Headquarters.Services.Impl
{
    public class InterviewerVersionReader : IInterviewerVersionReader
    {
        private readonly IClientApkProvider clientApkProvider;

        public InterviewerVersionReader(IClientApkProvider clientApkProvider)
        {
            this.clientApkProvider = clientApkProvider;
        }

        public int? Version => this.clientApkProvider.GetLatestVersion(ClientApkInfo.InterviewerFileName);
        public int? SupervisorVersion => this.clientApkProvider.GetLatestVersion(ClientApkInfo.SupervisorFileName);
    }
}
