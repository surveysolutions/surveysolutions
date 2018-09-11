using Autofac;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.Infrastructure.Native.Logging;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Versions;

namespace WB.UI.WebTester
{
    public class AutofacConfig
    {
        public static ContainerBuilder CreateKernel()
        {
            ContainerBuilder builder = new ContainerBuilder();
            
            builder.RegisterModule(new NcqrsModule().AsAutofac());
            builder.RegisterModule(new NLogLoggingModule().AsAutofac());
            builder.RegisterModule(new EventSourcedInfrastructureModule().AsAutofac());
            builder.RegisterModule(new InfrastructureModuleMobile().AsAutofac());
            builder.RegisterModule(new DataCollectionSharedKernelModule().AsAutofac());
            builder.RegisterModule(new CaptchaModule("recaptcha").AsAutofac());
            builder.RegisterModule(new WebInterviewModule().AsAutofac());
            builder.RegisterModule(new WebTesterModule().AsAutofac());
            builder.RegisterModule(new ProductVersionModule(typeof(Startup).Assembly, shouldStoreVersionToDb: false).AsAutofac());

            return builder;
        }
    }
}
