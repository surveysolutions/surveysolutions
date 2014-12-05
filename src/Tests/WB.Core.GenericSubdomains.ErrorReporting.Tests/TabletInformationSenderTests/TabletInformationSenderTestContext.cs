using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.GenericSubdomains.ErrorReporting.Tests.TabletInformationSenderTests
{
    internal class TabletInformationSenderTestContext
    {
        protected static TabletInformationSender CreateTabletInformationSender(bool isNetworkEnabled = true, string pathToInfoArchive = "",
            bool isSentSuccessfully = false)
        {
            return new TabletInformationSender(
                Mock.Of<ICapiInformationService>(_ => _.CreateInformationPackage() == pathToInfoArchive),
                Mock.Of<INetworkService>(_ => _.IsNetworkEnabled() == isNetworkEnabled),
                Mock.Of<IFileSystemAccessor>(_ =>
                    _.IsFileExists(It.IsAny<string>()) == true
                        && _.ReadAllBytes(It.IsAny<string>()) == new byte[0]),
                Mock.Of<IJsonUtils>(), string.Empty, string.Empty, string.Empty,
                Mock.Of<IRestServiceWrapperFactory>(_ => _.CreateRestServiceWrapper(
                    Moq.It.IsAny<string>(), Moq.It.IsAny<bool>()) ==
                    Mock.Of<IRestServiceWrapper>(r_ => r_.ExecuteRestRequestAsync<bool>(It.IsAny<string>(), It.IsAny<CancellationToken>(),
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) == Task.FromResult(isSentSuccessfully))));
        }

        protected static bool WaitUntilOperationEndsReturnFalseIfCanceled(TabletInformationSender tabletInformationSender,
            Action<TabletInformationSender> action)
        {
            var result = false;
            var remoteCommandDoneEvent = new AutoResetEvent(false);
            tabletInformationSender.ProcessCanceled += (s, e) =>
            {
                result = false;
                remoteCommandDoneEvent.Set();
            };
            tabletInformationSender.ProcessFinished += (s, e) =>
            {
                result = true;
                remoteCommandDoneEvent.Set();
            };
            action(tabletInformationSender);
            remoteCommandDoneEvent.WaitOne();
            return result;
        }
    }
}
