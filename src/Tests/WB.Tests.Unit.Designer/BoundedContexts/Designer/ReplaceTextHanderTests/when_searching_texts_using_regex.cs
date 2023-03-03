using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_searching_texts_using_regex : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticTextAndMoveIfNeeded(new AddStaticText(
                questionnaire.Id,
                responsibleId:responsibleId,
                entityId: staticTextId,
                text: "static text 5 title with",
                parentId: chapterId));
            BecauseOf();
        }

        private void BecauseOf() => matches = questionnaire.FindAllTexts("\\d", false, false, true);

        [NUnit.Framework.Test] 
        public void should_find_text_using_regex () => 
            matches.Should().Contain(x => x.Id == staticTextId);

        static Questionnaire questionnaire;

        static readonly Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid staticTextId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static IEnumerable<QuestionnaireEntityReference> matches;
    }
} 
