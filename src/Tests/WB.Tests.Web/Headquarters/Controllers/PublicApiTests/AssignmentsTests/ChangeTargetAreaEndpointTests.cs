using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class ChangeTargetAreaEndpointTests : BaseAssignmentsControllerTest
    {
        [Test]
        public void when_target_area_is_64_chars_should_dispatch_command()
        {
            var assignment = Create.Entity.Assignment(id: 1);
            this.SetupAssignment(assignment);
            var targetArea = new string('a', AssignmentConstants.TargetAreaLengthLimit);

            var result = this.controller.ChangeTargetArea(assignment.Id, targetArea);

            commandService.Verify(x =>
                x.Execute(It.Is<UpdateAssignmentTargetArea>(c => c.TargetArea == targetArea), null),
                Times.Once);
        }

        [Test]
        public void when_target_area_is_65_chars_should_return_406_without_dispatching_command()
        {
            var assignment = Create.Entity.Assignment(id: 1);
            this.SetupAssignment(assignment);
            var targetArea = new string('a', AssignmentConstants.TargetAreaLengthLimit + 1);

            var result = this.controller.ChangeTargetArea(assignment.Id, targetArea);

            Assert.That(result.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode))
                .EqualTo(StatusCodes.Status406NotAcceptable));
            commandService.Verify(x =>
                x.Execute(It.IsAny<UpdateAssignmentTargetArea>(), null),
                Times.Never);
        }
    }
}
