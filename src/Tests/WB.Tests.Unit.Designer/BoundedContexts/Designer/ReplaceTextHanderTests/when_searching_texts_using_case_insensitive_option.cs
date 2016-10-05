using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_searching_texts_using_case_insensitive_option : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticText(Create.Event.StaticTextAdded(entityId: staticTextId,
                text: $"static text title with {searchFor}",
                parentId: chapterId));
        };

        Because of = () => matches = questionnaire.FindAllTexts(searchFor.ToLower(), false);

        It should_find_ignoring_casing = () => matches.ShouldContain(x => x.Id == staticTextId);

        static Questionnaire questionnaire;

        static Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid staticTextId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static string searchFor = "With Casing";
        static IEnumerable<QuestionnaireNodeReference> matches;
    }
}