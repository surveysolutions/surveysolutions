using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    [TestOf(typeof(PlainQuestionnaire))]
    public class PlainQuestionnaireSubstitutionsTests
    {
        [Test]
        public void when_getting_groups_affected_by_substitutions_in_fixed_title_from_outer_scope_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.g1, variable: "text"),
                    Create.Entity.Roster(Id.g2, rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new []{ "test", "start %text% and %inner_text% end" },
                        children: new List<IComposite>
                        {
                            Create.Entity.TextQuestion(Id.g3, variable: "inner_text"),
                        })
                });

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire, version: 1, substitutionService: new SubstitutionService());
            var affectedQuestions = plainQuestionnaire.GetSubstitutedGroups(Id.g1);

            affectedQuestions.Should().Contain(Id.g2);
        }

        [Test]
        public void when_getting_groups_affected_by_substitutions_in_fixed_title_from_inner_scope_question()
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(
                children: new IComposite[]
                {
                    Create.Entity.TextQuestion(Id.g1, variable: "text"),
                    Create.Entity.Roster(Id.g2, rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new []{ "test", "start %text% and %inner_text% end" },
                        children: new List<IComposite>
                        {
                            Create.Entity.TextQuestion(Id.g3, variable: "inner_text"),
                        })
                });

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaire, version: 1, substitutionService: new SubstitutionService());
            var affectedQuestions = plainQuestionnaire.GetSubstitutedGroups(Id.g3);

            affectedQuestions.Should().Contain(Id.g2);
        }
    }
}