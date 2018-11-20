using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_questions_affected_by_substitutions : PlainQuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var rosterSizeId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitleid = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            substitutionTargetQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
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

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire, version: 1, substitutionService: new SubstitutionService());

            BecauseOf();
        }  

        public void BecauseOf() => 
            affectedQuestions = plainQuestionnaire.GetSubstitutedQuestions(rosterTitleid);

        [NUnit.Framework.Test] public void should_find_roster_title_substitutions () => 
            affectedQuestions.Should().Contain(substitutionTargetQuestionId);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid substitutionTargetQuestionId;
        private static Guid rosterTitleid;
        private static IEnumerable<Guid> affectedQuestions;
    }
}

