using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;
using WB.Core.GenericSubdomains.ErrorReporting.Services;
using WB.Core.GenericSubdomains.ErrorReporting.Services.TabletInformationSender;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.GenericSubdomains.ErrorReporting.TabletInformationSenderTests
{
    internal class when_archiving_process_thrown_an_exception : TabletInformationSenderTestContext
    {
        Establish context = () =>
        {
            var errorReportingSettingsMock = new Mock<IErrorReportingSettings>();

            errorReportingSettingsMock.Setup(x => x.GetClientRegistrationId())
                .Throws(new NullReferenceException(exceptionMessage));
            tabletInformationSender = CreateTabletInformationSender(errorReportingSettings: errorReportingSettingsMock.Object);
            tabletInformationSender.ProcessCanceled += (s, e) => { cancellationEvent = e; };
        };

        Because of = () => isOperationFinished = WaitUntilOperationEndsReturnFalseIfCanceled(tabletInformationSender, t => t.Run());

        It should_operation_be_finished = () => isOperationFinished.ShouldEqual(false);

        It should_rise_CancellationEvent = () => cancellationEvent.ShouldNotBeNull();

        It should_rise_CancellationEvent_with_reason_equal_exception_message = () => cancellationEvent.Reason.ShouldEqual(exceptionMessage);

        private static TabletInformationSender tabletInformationSender;
        private static bool isOperationFinished;
        private static string exceptionMessage = "hello world";
        private static InformationPackageCancellationEventArgs cancellationEvent;
    }
}
