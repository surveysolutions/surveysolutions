using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_list_assignments_with_AssignmentsPublicApi : BaseAssignmentsControllerTest
    {
        public override void Setup()
        {
            base.Setup();

            this.assignmentViewFactory.Setup(avf => avf.Load(It.IsAny<AssignmentsInputModel>()))
                .Returns(new AssignmentsWithoutIdentifingData());
        }

        [Test]
        public void should_limit_list_to_20_by_default()
        {
            this.controller.List(null);

            this.assignmentViewFactory.Verify(l => l.Load(It.Is< AssignmentsInputModel>(
                aim => aim.Limit == 20)));
        }

        [Test]
        public void should_limit_list_to_100_at_max()
        {
            this.controller.List(new AssignmentsListFilter{ Limit = 1000 });

            this.assignmentViewFactory.Verify(l => l.Load(It.Is< AssignmentsInputModel>(
                aim => aim.Limit == 100)));
        }

        [Test]
        public void should_use_0_offset_by_default()
        {
            this.controller.List(null);

            this.assignmentViewFactory.Verify(l => l.Load(It.Is<AssignmentsInputModel>(
                aim => aim.Offset == 0)));
        }
    }
}