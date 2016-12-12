using System;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal partial class StatefullInterviewTests
    {
        [Test]
        public void When_question_group_and_roster_have_substitutions_Should_empty_substitutions_replaced_to_ellipsis_with_dots()
        {
            //arrange
            var questionWithSubstitutionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var groupWithSubstitutionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterWithSubsititutionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(variable: "sourceOfSubstitution"),
                Create.Entity.TextQuestion(questionWithSubstitutionId, text: "question with %sourceOfSubstitution%"),
                Create.Entity.Group(groupWithSubstitutionId, title: "group with %sourceOfSubstitution%"),
                Create.Entity.FixedRoster(rosterWithSubsititutionId, title: "roster with %sourceOfSubstitution%",
                    fixedTitles: new[] {Create.Entity.FixedTitle(1, "roster title")}));

            //act
            var interview = Setup.StatefulInterview(questionnaire);

            //assert
            Assert.That(interview.GetTitleText(Identity.Create(questionWithSubstitutionId, RosterVector.Empty)), Is.EqualTo("question with [...]"));
            Assert.That(interview.GetTitleText(Identity.Create(groupWithSubstitutionId, RosterVector.Empty)), Is.EqualTo("group with [...]"));
            Assert.That(interview.GetTitleText(Identity.Create(rosterWithSubsititutionId, Create.Entity.RosterVector(1))), Is.EqualTo("roster with [...]"));
        }
    }
}