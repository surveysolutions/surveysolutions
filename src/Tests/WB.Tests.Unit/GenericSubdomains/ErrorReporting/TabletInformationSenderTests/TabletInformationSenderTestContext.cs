using System;
using System.Threading;
using Moq;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;
using WB.Core.GenericSubdomains.ErrorReporting.Services;
using WB.Core.GenericSubdomains.ErrorReporting.Services.CapiInformationService;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Tests.Unit.GenericSubdomains.ErrorReporting.TabletInformationSenderTests
{
    internal class TabletInformationSenderTestContext
    {
        protected static TabletInformationSender CreateTabletInformationSender(bool isNetworkEnabled = true,
            string pathToInfoArchive = "",
            bool isSentSuccessfully = false, IErrorReportingSettings errorReportingSettings = null)
        {
            return new TabletInformationSender(
                Mock.Of<ICapiInformationService>(_ => _.CreateInformationPackage() == pathToInfoArchive),
                Mock.Of<IFileSystemAccessor>(_ =>
                    _.IsFileExists(It.IsAny<string>()) == true
                    && _.ReadAllBytes(It.IsAny<string>()) == new byte[0]),
                Mock.Of<IRestService>(),
                errorReportingSettings??Mock.Of<IErrorReportingSettings>(),
                Mock.Of<ILogger>());
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
