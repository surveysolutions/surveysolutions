using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    [TestOf(typeof(PlainQuestionnaire))]
    [Ignore("KP-8526")]
    internal class when_getting_questions_affected_by_substitutions
    {
        [Test]
        public void should_find_roster_title_substitutions()
        {
            // arrange 
            var rosterSizeId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterTitleid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var substitutionTargetQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaire = Create.Entity.QuestionnaireDocument(
                children: new List<IComposite>
                {
                    Create.Entity.NumericIntegerQuestion(rosterSizeId),
                    Create.Entity.Roster(rosterSizeQuestionId: rosterSizeId,
                        rosterTitleQuestionId: rosterTitleid,
                        children: new List<IComposite>
                        {
                            Create.Entity.TextQuestion(questionId: rosterTitleid),
                            Create.Entity.TextQuestion(questionId: substitutionTargetQuestionId, text: "with %rostertitle%")
                        })
                });

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire);

            // Act
            var affectedQuestions = plainQuestionnaire.GetSubstitutedQuestions(rosterTitleid);

            // Assert
            Assert.That(affectedQuestions, Does.Contain(substitutionTargetQuestionId));
        }
    }
}

