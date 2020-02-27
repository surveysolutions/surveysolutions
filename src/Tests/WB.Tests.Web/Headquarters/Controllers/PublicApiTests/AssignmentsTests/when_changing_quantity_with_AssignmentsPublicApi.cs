using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_changing_quantity_with_AssignmentsPublicApi : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_404_for_non_existing_assignment()
        {
            var result = this.controller.ChangeQuantity(101, -1);
            Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
        }
        
        [Test]
        public void should_store_updated_quantity()
        {
            this.SetupAssignment(Create.Entity.Assignment(id: 1, publicKey: Id.g1, quantity: 10));

            mapper.Setup(x => x.Map<AssignmentDetails>(It.IsAny<Assignment>()))
                .Returns((Assignment arg) =>
                {
                    return new AssignmentDetails
                    {
                        Id = arg.Id,
                        Quantity = arg.Quantity
                    };
                });

            this.controller.ChangeQuantity(1, 30);

            this.commandService.Verify(x => x.Execute(It.Is<UpdateAssignmentQuantity>(a => a.Quantity == 30 && a.PublicKey == Id.g1), null), Times.Once);
        }
    }
}
