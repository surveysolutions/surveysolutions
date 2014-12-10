using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;

namespace WB.Tests.Unit.GenericSubdomains.ErrorReporting.TabletInformationSenderTests
{
    internal class when_network_is_disabled : TabletInformationSenderTestContext
    {
        Establish context = () =>
        {
            tabletInformationSender = CreateTabletInformationSender(false);
        };

        Because of = () => isOperationCanceled = WaitUntilOperationEndsReturnFalseIfCanceled(tabletInformationSender, t => t.Run());

        It should_operation_be_canceled = () => isOperationCanceled.ShouldEqual(false);

        private static TabletInformationSender tabletInformationSender;
        private static bool isOperationCanceled;
    }
}
