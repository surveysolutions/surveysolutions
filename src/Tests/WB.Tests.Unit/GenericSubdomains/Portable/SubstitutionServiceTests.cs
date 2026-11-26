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

        [Test]
        public void when_text_contains_at_prefixed_variable_should_extract_it()
        {
            var service = Create.Service.SubstitutionService();

            string[] names = service.GetAllSubstitutionVariableNames("code: %@rowcode%", null);

            Assert.That(names, Is.EquivalentTo(new[] { "@rowcode" }));
        }

        [Test]
        public void when_text_contains_rowcode_without_at_should_extract_it()
        {
            var service = Create.Service.SubstitutionService();

            string[] names = service.GetAllSubstitutionVariableNames("code: %rowcode%", null);

            Assert.That(names, Is.EquivalentTo(new[] { "rowcode" }));
        }

        [TestCase("@rowcode", true)]
        [TestCase("rowcode", true)]
        [TestCase("@rowindex", true)]
        [TestCase("rowindex", true)]
        [TestCase("@rowname", true)]
        [TestCase("rowname", true)]
        [TestCase("myvar", false)]
        [TestCase("rostertitle", false)]
        public void IsRosterServiceVariable_should_correctly_identify_service_variables(string variableName, bool expected)
        {
            var service = Create.Service.SubstitutionService();

            Assert.That(service.IsRosterServiceVariable(variableName), Is.EqualTo(expected));
        }
    }
}
