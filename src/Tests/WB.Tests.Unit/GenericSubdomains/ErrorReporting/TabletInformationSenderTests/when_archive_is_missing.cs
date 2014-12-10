using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using WB.Core.GenericSubdomains.ErrorReporting.Implementation.TabletInformation;

namespace WB.Tests.Unit.GenericSubdomains.ErrorReporting.TabletInformationSenderTests
{
    internal class when_archive_is_missing : TabletInformationSenderTestContext
    {
        Establish context = () =>
        {
            tabletInformationSender = CreateTabletInformationSender();
        };

        Because of = () => isOperationCanceled = WaitUntilOperationEndsReturnFalseIfCanceled(tabletInformationSender, t => t.Run());

        It should_operation_be_finished = () => isOperationCanceled.ShouldEqual(true);

        private static TabletInformationSender tabletInformationSender;
        private static bool isOperationCanceled;
    }
}
