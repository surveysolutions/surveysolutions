using Machine.Specifications;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;

namespace WB.Tests.Unit.GenericSubdomains.ErrorReporting.TabletInformationSenderTests
{
    internal class when_archive_wasnt_sent_successfully_to_supervisor : TabletInformationSenderTestContext
    {
        Establish context = () =>
        {
            tabletInformationSender = CreateTabletInformationSender(true, "archive");
        };

        Because of = () => isOperationCanceled = WaitUntilOperationEndsReturnFalseIfCanceled(tabletInformationSender, t => t.Run());

        It should_operation_be_canceled = () => isOperationCanceled.ShouldEqual(false);

        private static TabletInformationSender tabletInformationSender;
        private static bool isOperationCanceled;
    }
}
