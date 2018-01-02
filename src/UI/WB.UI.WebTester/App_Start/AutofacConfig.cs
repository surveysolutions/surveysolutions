using System.Reflection;
using Autofac;
using Autofac.Integration.SignalR;
using WB.Core.BoundedContexts.Tester;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Shared.Web.Captcha;
using WB.UI.WebTester.Hub;

namespace WB.UI.WebTester
{
    public class AutofacConfig
    {
        public static ContainerBuilder CreateKernel()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new NcqrsModule().AsAutofac());
            builder.RegisterModule(new EventSourcedInfrastructureModule().AsAutofac());
            builder.RegisterModule(new InfrastructureModuleMobile().AsAutofac());
            builder.RegisterModule(new DataCollectionSharedKernelModule().AsAutofac());
            builder.RegisterModule(new TesterBoundedContextModule().AsAutofac());
            builder.RegisterModule(new CaptchaModule("recaptcha").AsAutofac());
            builder.RegisterModule(new WebTesterModule().AsAutofac());
            builder.RegisterModule(new WebInterviewModule().AsAutofac());

            return builder;
        }
    }
}