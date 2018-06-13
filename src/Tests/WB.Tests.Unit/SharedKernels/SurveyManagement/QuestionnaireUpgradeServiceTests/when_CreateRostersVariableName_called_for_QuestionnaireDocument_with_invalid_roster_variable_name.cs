using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireUpgradeServiceTests
{
    internal class when_CreateRostersVariableName_called_for_QuestionnaireDocument_with_invalid_roster_variable_name : QuestionnaireUpgradeServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new Group("roster with empty var name") { PublicKey = rosterWithEmptyVarName, IsRoster = true},
                new Group(questionnaireTitle) { PublicKey = rosterTitleEqualToQuestionnaireTitle, IsRoster = true },
                new Group("все символы не читабельные") { PublicKey = rosterAllSymbolsAreUnreadle, IsRoster = true },
                new Group("When variable name will be longer then 32 symbols") { PublicKey = rosterWhenNewVeariableNameLongerThen32, IsRoster = true },
                new Group("1 when first title symbol is numeric") { PublicKey = rosterWhenFirstSymbolInTitleIsNumeric, IsRoster = true },
                new Group("when variable name is too long") { PublicKey = rosterWhenVariableNameIsTooLong, VariableName = "a123456789012345678901234567890123", IsRoster = true },
                new Group("when variable with invalid characters") { PublicKey = rosterWhenVariableNameContainsInvalidCharacters, VariableName = "varневалид", IsRoster = true },
                new Group("when variable is valid") { PublicKey = rosterWhenVariableNameIsValid, VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = rosterWhenVariableNameIsDublicate, VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = Guid.NewGuid(), VariableName = var1, IsRoster = true },
                new Group("when variable is dublicate") { PublicKey = rosterWhenVariableNameIsDublicateFor11thTime, VariableName = var1, IsRoster = true }
                );
            questionnaireDocument.Title = questionnaireTitle;
            questionnaireUpgradeService = CreateQuestionnaireUpgradeService();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = questionnaireUpgradeService.CreateRostersVariableName(questionnaireDocument);

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__rosterwithemptyvarname__ () =>
            result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWithEmptyVarName).VariableName.Should().Be("rosterwithemptyvarname");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__title1__ () =>
            result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterTitleEqualToQuestionnaireTitle).VariableName.Should().Be("title1");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__roster__ () =>
           result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterAllSymbolsAreUnreadle).VariableName.Should().Be("roster");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__whenvariablenamewillbelongerthen__ () =>
            result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWhenNewVeariableNameLongerThen32).VariableName.Should().Be("whenvariablenamewillbelongerthen");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__whenfirsttitlesymbolisnumeric__ () =>
           result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWhenFirstSymbolInTitleIsNumeric).VariableName.Should().Be("whenfirsttitlesymbolisnumeric");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__a1234567890123456789012345678901__ () =>
         result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWhenVariableNameIsTooLong).VariableName.Should().Be("a1234567890123456789012345678901");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__var__ () =>
            result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWhenVariableNameContainsInvalidCharacters).VariableName.Should().Be("var");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__var012345678901234567890123last__ () =>
           result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWhenVariableNameIsValid).VariableName.Should().Be(var1);

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__var012345678901234567890123last1__ () =>
          result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWhenVariableNameIsDublicate).VariableName.Should().Be("var012345678901234567890123last1");

        [NUnit.Framework.Test] public void should_result_has_roster_with_variable_name__var110__ () =>
            result.FirstOrDefault<IGroup>(g => g.PublicKey == rosterWhenVariableNameIsDublicateFor11thTime).VariableName.Should().Be("var012345678901234567890123las10");

        private static QuestionnaireUpgradeService questionnaireUpgradeService;
        private static QuestionnaireDocument questionnaireDocument;
        private static QuestionnaireDocument result;
        private static string questionnaireTitle = "title";
        private static string var1 = "var012345678901234567890123last";
        private static Guid rosterWithEmptyVarName = Guid.NewGuid();
        private static Guid rosterTitleEqualToQuestionnaireTitle = Guid.NewGuid();
        private static Guid rosterAllSymbolsAreUnreadle = Guid.NewGuid();
        private static Guid rosterWhenNewVeariableNameLongerThen32 = Guid.NewGuid();
        private static Guid rosterWhenFirstSymbolInTitleIsNumeric = Guid.NewGuid();
        private static Guid rosterWhenVariableNameIsTooLong = Guid.NewGuid();
        private static Guid rosterWhenVariableNameContainsInvalidCharacters = Guid.NewGuid();
        private static Guid rosterWhenVariableNameIsValid = Guid.NewGuid();
        private static Guid rosterWhenVariableNameIsDublicate = Guid.NewGuid();
        private static Guid rosterWhenVariableNameIsDublicateFor11thTime = Guid.NewGuid();
    }
}
