using System.Web.Hosting;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Headquarters.API;

namespace WB.UI.Headquarters.Code
{
    class InterviewerVersionReader : IInterviewerVersionReader
    {
        private readonly IAndroidPackageReader androidPackageReader;
        private readonly IFileSystemAccessor fileSystemAccessor;

        public InterviewerVersionReader(IAndroidPackageReader androidPackageReader, IFileSystemAccessor fileSystemAccessor)
        {
            this.androidPackageReader = androidPackageReader;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public int? Version
        {
            get
            {
                string pathToInterviewerApp = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(ClientApkInfo.Directory), ClientApkInfo.InterviewerFileName);

                int? interviewerApkVersion = !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                    ? null
                    : this.androidPackageReader.Read(pathToInterviewerApp).Version;

                return interviewerApkVersion;
            }
        }
    }
}
