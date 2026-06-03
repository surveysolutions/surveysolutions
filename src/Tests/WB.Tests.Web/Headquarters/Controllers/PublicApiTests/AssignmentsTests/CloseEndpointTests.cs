using System;
using System.Reflection;
using NUnit.Framework;
using WB.UI.Headquarters.Controllers.Api.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    public class CloseEndpointTests : BaseAssignmentsControllerTest
    {
        [Test]
        public void should_have_obsolete_attribute_for_post()
        {
            var method = typeof(AssignmentsController).GetMethod(nameof(AssignmentsController.ClosePost));
            var attribute = method.GetCustomAttribute<ObsoleteAttribute>();
            
            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.Message, Is.EqualTo("Use 'downsize' or 'changeStatus' instead"));
        }
        
        [Test]
        public void should_have_obsolete_attribute_for_patch()
        {
            var method = typeof(AssignmentsController).GetMethod(nameof(AssignmentsController.Close));
            var attribute = method.GetCustomAttribute<ObsoleteAttribute>();
            
            Assert.That(attribute, Is.Not.Null);
            Assert.That(attribute.Message, Is.EqualTo("Use 'downsize' or 'changeStatus' instead"));
        }
    }
}
