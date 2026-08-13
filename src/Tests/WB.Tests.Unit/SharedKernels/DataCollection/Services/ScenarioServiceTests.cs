using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Scenarios;

namespace WB.Tests.Unit.SharedKernels.DataCollection.Services
{
    [TestOf(typeof(ScenarioService))]
    public class ScenarioServiceTests
    {
        [Test]
        public void ConvertFromScenario_when_switch_translation_language_does_not_exist_should_skip_command()
        {
            var questionnaire = Mock.Of<IQuestionnaire>(q =>
                q.GetTranslationLanguages() == new[] { "English" }.ToList().AsReadOnly());

            var scenarioCommands = new List<IScenarioCommand>
            {
                new SwitchTranslation("Deutsch")
            };

            var service = new ScenarioService();
            var result = service.ConvertFromScenario(questionnaire, scenarioCommands);

            Assert.That(result, Is.Empty,
                "SwitchTranslation command with non-existent language should be skipped during scenario replay");
        }

        [Test]
        public void ConvertFromScenario_when_switch_translation_language_exists_should_include_command()
        {
            var questionnaire = Mock.Of<IQuestionnaire>(q =>
                q.GetTranslationLanguages() == new[] { "Deutsch" }.ToList().AsReadOnly());

            var scenarioCommands = new List<IScenarioCommand>
            {
                new SwitchTranslation("Deutsch")
            };

            var service = new ScenarioService();
            var result = service.ConvertFromScenario(questionnaire, scenarioCommands);

            Assert.That(result, Has.Count.EqualTo(1),
                "SwitchTranslation command with existing language should be included during scenario replay");
        }

        [Test]
        public void ConvertFromScenario_when_switch_translation_to_default_language_should_include_command()
        {
            var questionnaire = Mock.Of<IQuestionnaire>(q =>
                q.GetTranslationLanguages() == new[] { "Deutsch" }.ToList().AsReadOnly());

            var scenarioCommands = new List<IScenarioCommand>
            {
                new SwitchTranslation(null)
            };

            var service = new ScenarioService();
            var result = service.ConvertFromScenario(questionnaire, scenarioCommands);

            Assert.That(result, Has.Count.EqualTo(1),
                "SwitchTranslation to default language (null) should always be included during scenario replay");
        }
    }
}
