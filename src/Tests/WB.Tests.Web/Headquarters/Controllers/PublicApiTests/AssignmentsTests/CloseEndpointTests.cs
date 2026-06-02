using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class CloseEndpointTests : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_return_400_with_obsolete_message_for_post()
        {
            var result = this.controller.ClosePost(14);
            
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.Value, Is.EqualTo("The 'close' endpoint is obsolete. Use 'downsize' or 'changeStatus' depending on your needs."));
        }
        
        [Test]
        public void should_return_400_with_obsolete_message_for_patch()
        {
            var result = this.controller.Close(14).Result;
            
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = (BadRequestObjectResult)result;
            Assert.That(badRequest.Value, Is.EqualTo("The 'close' endpoint is obsolete. Use 'downsize' or 'changeStatus' depending on your needs."));
        }
    }
}
