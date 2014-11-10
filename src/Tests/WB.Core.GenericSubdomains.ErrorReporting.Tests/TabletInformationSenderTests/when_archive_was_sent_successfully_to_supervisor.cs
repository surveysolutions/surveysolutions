using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;

namespace WB.Core.GenericSubdomains.ErrorReporting.Tests.TabletInformationSenderTests
{
    [Ignore("Fix tomorrow")]
    class when_archive_was_sent_successfully_to_supervisor : TabletInformationSenderTestContext
    {
        Establish context = () =>
        {
            tabletInformationSender = CreateTabletInformationSender(true, "archive", true);
        };

        Because of = () => isOperationCanceled = WaitUntilOperationEndsReturnFalseIfCanceled(tabletInformationSender, t => t.Run());

        It should_operation_be_finished = () => isOperationCanceled.ShouldEqual(true);

        private static TabletInformationSender tabletInformationSender;
        private static bool isOperationCanceled;
    }
}
