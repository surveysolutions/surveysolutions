using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.Questionnaire
{
    public class QuestionnaireTests
    {
        [Test]
        public void should_return_groups_with_questions()
        {
            var questionnaire = Create.QuestionnaireDocument(Id.gA, variableName: "quest",
                children: new IQuestionnaireEntity[]
                {
                    Create.Group(Id.g1, variable: "section1", children: new IQuestionnaireEntity[]
                    {
                        Create.NumericIntegerQuestion(Id.g2, variable: "n1"),
                        Create.Roster(Id.g3, variable: "rost1", rosterSizeQuestionId: Id.g2,
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            children: new IQuestionnaireEntity[]
                            {
                                Create.TextQuestion(Id.g4, variable: "txt")
                            })
                    }),
                    Create.Group(Id.g5,
                        children:
                            Create.Group(Id.g6,
                                children: Create.TextQuestion()
                    ))
                });

            // Act
            var storedGroups = questionnaire.GetAllStoredGroups();

            // Assert
            Assert.That(storedGroups.Select(x => x.PublicKey), Is.EquivalentTo(new[] { Id.g1, Id.g3, Id.g6 }), "Groups with questions should be returned");
        }

        [Test]
        public void IsInsideRoster_should_return_true_for_group_within_roster()
        {
            var target = Create.Group();
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(Id.gA, 
                children: new IQuestionnaireEntity[]
                {
                    Create.NumericIntegerQuestion(Id.g2, variable: "n1"),
                    Create.Roster(Id.g3, variable: "rost1", rosterSizeQuestionId: Id.g2,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        children: new IQuestionnaireEntity[]
                        {
                            target
                        })
                });
            questionnaire.ConnectChildrenWithParent();

            // Act 
            var targetIsInsideRoster = target.IsInsideRoster;

            // Assert
            Assert.That(targetIsInsideRoster);
        }
    }
}
