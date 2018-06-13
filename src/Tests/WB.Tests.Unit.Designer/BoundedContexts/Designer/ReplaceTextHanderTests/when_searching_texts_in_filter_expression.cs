using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_searching_texts_in_filter_expression : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddMultiOptionQuestion(filteredQuestionId, chapterId, responsibleId,
                optionsFilterExpression: $"filter with {searchFor}");
            BecauseOf();
        }

        private void BecauseOf() => matches = questionnaire.FindAllTexts(searchFor, false, false, false);

        [NUnit.Framework.Test] public void should_find_text_in_filter_expression () =>
            matches.Should().Contain(x => x.Id == filteredQuestionId &&
                                       x.Property == QuestionnaireVerificationReferenceProperty.OptionsFilter);

        static Questionnaire questionnaire;

        const string searchFor = "to_search";
        static readonly Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid filteredQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static IEnumerable<QuestionnaireEntityReference> matches;
    }
}
