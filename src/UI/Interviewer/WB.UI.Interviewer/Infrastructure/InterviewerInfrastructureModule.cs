using Ninject.Modules;
using Sqo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Infrastructure.Shared.Enumerator;
using WB.UI.Interviewer.Infrastructure.Internals.Security;
using WB.UI.Interviewer.Settings;
using InterviewerSettings = WB.UI.Interviewer.Infrastructure.Internals.Settings.InterviewerSettings;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            SiaqodbConfigurator.SetLicense(@"yrwPAibl/TwJ+pR5aBOoYieO0MbZ1HnEKEAwjcoqtdrUJVtXxorrxKZumV+Z48/Ffjj58P5pGVlYZ0G1EoPg0w==");
            this.Bind<ISiaqodb>().ToConstant(new Siaqodb(AndroidPathUtils.GetPathToSubfolderInLocalDirectory("database")));

            this.Bind(typeof(IAsyncPlainStorage<>)).To(typeof(SiaqodbPlainStorage<>)).InSingletonScope();

            this.Bind<IEnumeratorSettings>().To<InterviewerSettings>();
            this.Bind<IPrincipal>().To<InterviewerPrincipal>().InSingletonScope();
            this.Bind<IRestServiceSettings>().To<RestServiceSettings>();
        }
    }
}