using System.Threading.Tasks;
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

        public Task<int?> InterviewerBuildNumber() => this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.InterviewerFileName);

        public Task<int?> SupervisorBuildNumber() => this.clientApkProvider.GetApplicationBuildNumber(ClientApkInfo.SupervisorFileName);
    }
}
