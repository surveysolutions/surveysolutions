using System;
using NUnit.Framework;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire
{
    public class QuestionnaireTests
    {
        [Test]
        public void MainScopeSections_Should_return_sections_list_that_should_be_exported_to_main_file()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocument(Id.g1, "questionnaire",
                Create.Group(Id.g2, Create.Group(Id.g3)),
                Create.Group(Id.g4, Create.Group(Id.g5), 
                    Create.Roster(Id.g6, children: new []{Create.Group(Id.g7)})));

            // Act
            var mainScope = questionnaire.GetMainScopeSections();

            // Assert
            Assert.That(mainScope, Is.EquivalentTo(new[]{Id.g2, Id.g3, Id.g4, Id.g5}));
        }
    }
}
