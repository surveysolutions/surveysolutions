using System.Net;
using System.Net.Http;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_changing_quantity_with_AssignmentsPublicApi : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_404_for_non_existing_assignment()
        {
            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound),
                () => this.controller.ChangeQuantity(101, null));
        }
        
        [Test]
        public void should_store_updated_quantity()
        {
            this.SetupAssignment(Create.Entity.Assignment(id: 1, quantity: 10));

            this.controller.ChangeQuantity(1, 30);

            this.assignmentsStorage.Verify(x => x.Store(It.Is<Assignment>(a => a.Quantity == 30), 1), Times.Once);
        }
    }
}