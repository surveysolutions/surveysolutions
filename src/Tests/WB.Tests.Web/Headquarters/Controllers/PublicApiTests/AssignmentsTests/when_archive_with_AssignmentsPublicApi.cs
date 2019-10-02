using System.Net;
using System.Net.Http;
using System.Web.Http;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_archive_with_AssignmentsPublicApi : BaseAssignmentsControllerTest
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
        public void should_store_updated_archive_status()
        {
            this.SetupAssignment(Create.Entity.Assignment(id: 1, publicKey: Id.g7, quantity: 10));

            this.controller.Archive(1);

            this.commandService.Verify(x => x.Execute(It.Is<ArchiveAssignment>(a => a.PublicKey == Id.g7), null), Times.Once);
        }

        [Test]
        public void should_store_updated_unarchive_status()
        {
            this.SetupAssignment(Create.Entity.Assignment(id: 1, publicKey: Id.g7, quantity: 10));

            this.controller.Unarchive(1);

            this.commandService.Verify(c => c.Execute(It.Is<UnarchiveAssignment>(a => a.PublicKey == Id.g7), null), Times.Once);
        }
    }
}
