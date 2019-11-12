using System.Net;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Commands.Assignment;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class when_assign_new_user_with_AssignmentsPublicApi : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_404_for_non_existing_assignment_on_details()
        {
            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound),
                () => this.controller.Details(101));
        }

        [Test]
        public void should_return_404_for_non_existing_assignment()
        {
            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound),
                () => this.controller.Assign(101, null));
        }
        
        [Test]
        public void should_return_404_for_non_existing_responsibleUser()
        {
            this.SetupAssignment(new Assignment());

            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotFound),
                () => this.controller.Assign(101, null));
        }

        [TestCase(UserRoles.Administrator)]
        [TestCase(UserRoles.Headquarter)]
        [TestCase(UserRoles.Observer)]
        [TestCase(UserRoles.ApiUser)]
        public void should_return_406_for_non_interviewer_supervisor_assignee(UserRoles role )
        {
            this.SetupAssignment(new Assignment());
            this.SetupResponsibleUser(Create.Entity.HqUser(role: role));
            
            Assert.Throws(Is.TypeOf<HttpResponseException>()
                    .And.Property(nameof(HttpResponseException.Response))
                    .Property(nameof(HttpResponseMessage.StatusCode)).EqualTo(HttpStatusCode.NotAcceptable),
                () => this.controller.Assign(101, new AssignmentAssignRequest(){Responsible = "any"}));
        }

        [Test]
        public void should_store_assignment_with_new_assignee()
        {
            this.SetupAssignment(Create.Entity.Assignment(id: 42, publicKey: Id.g7));

            var user = Create.Entity.HqUser(userId: Id.g1, role: UserRoles.Interviewer);
            this.SetupResponsibleUser(user);

            this.controller.Assign(42, new AssignmentAssignRequest()
            {
                Responsible = "any"
            });

            this.commandService.Verify(ass => ass.Execute(It.Is<ReassignAssignment>(a => a.ResponsibleId == Id.g1 && a.PublicKey == Id.g7), null), Times.Once );
        }
    }
}
