using Ninject;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.SharedKernels.DataCollection;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Shared.Web.Captcha;
using WB.UI.Shared.Web.Modules;

namespace WB.UI.WebTester
{
    public class NinjectConfig
    {
        public static IKernel CreateKernel()
        {
            var kernel = new StandardKernel(
                new NinjectSettings { InjectNonPublic = true },
                //new ServiceLocationModule(),
                new EventSourcedInfrastructureModule().AsNinject(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new CaptchaModule("recaptcha").AsNinject(),
                new WebTesterModule().AsNinject(),
                new DataCollectionSharedKernelModule().AsNinject(),
                new WebInterviewModule().AsNinject()
            );

            return kernel;
        }
    }
}