using NUnit.Framework;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.GenericSubdomains.Portable
{
    [TestOf(typeof(SubstitutionService))]
    public class SubstitutionServiceTests 
    {
        [Test]
        public void when_called_with_different_self_references_should_return_different_results()
        {
            var service = Create.Service.SubstitutionService();

            var questionText = "text %self%";
            string[] call1 = service.GetAllSubstitutionVariableNames(questionText, "m1");
            var call2 = service.GetAllSubstitutionVariableNames(questionText, "m2");

            Assert.That(call1, Is.EquivalentTo(new []{ "m1" }));
            Assert.That(call2, Is.EquivalentTo(new []{ "m2" }));
        }
    }
}
