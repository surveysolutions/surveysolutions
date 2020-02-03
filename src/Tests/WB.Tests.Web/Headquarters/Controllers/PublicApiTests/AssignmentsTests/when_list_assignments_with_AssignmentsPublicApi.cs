using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Controllers.Api.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    [TestOf(typeof(AssignmentsController))]
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
        
        [TestCase("Id", ExpectedResult = "Id ASC")]
        [TestCase("id", ExpectedResult = "Id ASC", Description = "Test for case insensitive")]
        [TestCase("id dEsc", ExpectedResult = "Id  Desc", Description = "Test for DESC ordering")]
        [TestCase("ResponsibleName", ExpectedResult = "Responsible.Name ASC", Description = "Test for special responsible name mapping")]
        public string should_map_order_values(string input)
        {
            string result = null;

            this.assignmentViewFactory
                .Setup(avf => avf.Load(It.IsAny<AssignmentsInputModel>()))
                .Returns(new AssignmentsWithoutIdentifingData())
                .Callback<AssignmentsInputModel>(model => result = model.Order);

            this.controller.List(new AssignmentsListFilter { Order = input });

            return result;
        }

        [Test]
        public async Task should_throw_406_on_incorrect_sort_expression()
        {
            var result = await this.controller.List(new AssignmentsListFilter { Order = "Nonexisting" });
            Assert.That(result.Result, Has.Property(nameof(IStatusCodeActionResult.StatusCode)).EqualTo(StatusCodes.Status406NotAcceptable));
        }
    }
}
