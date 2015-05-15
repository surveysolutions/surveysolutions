using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web.Mvc;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.UI.Supervisor.Controllers;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Supervisor.HQSyncControllerTests
{
    internal class when_requesting_push_status : HQSyncControllerTestsContext
    {
        Establish context = () =>
        {
            var headquartersPushContext = Mock.Of<HeadquartersPushContext>
            (_
                => _.IsRunning == isRunning
                && _.GetStatus() == status
                && _.GetMessages() == messages
                && _.GetErrors() == errors
            );

            controller = Create.HQSyncController(headquartersPushContext: headquartersPushContext);
        };

        Because of = () =>
            result = controller.PushStatus();

        It should_return_json_result = () =>
            result.ShouldBeOfExactType<JsonResult>();

        It should_return_is_running_flag_provided_by_push_context = () =>
            ShouldExtensionMethods.ShouldEqual(GetDataFromJsonResult(result).IsRunning, isRunning);


        It should_return_messages_provided_by_push_context = () =>
            ShouldExtensionMethods.ShouldEqual(GetDataFromJsonResult(result).Messages, messages);

        It should_return_errors_provided_by_push_context = () =>
            ShouldExtensionMethods.ShouldEqual(GetDataFromJsonResult(result).Errors, errors);

        private static ActionResult result;
        private static HQSyncController controller;
        private static bool isRunning = true;
        private static string status = "status";
        private static IReadOnlyCollection<string> messages = new ReadOnlyCollection<string>(new[] { "m1", "m2" });
        private static IReadOnlyCollection<string> errors = new ReadOnlyCollection<string>(new[] { "e1", "e2" });
    }
}