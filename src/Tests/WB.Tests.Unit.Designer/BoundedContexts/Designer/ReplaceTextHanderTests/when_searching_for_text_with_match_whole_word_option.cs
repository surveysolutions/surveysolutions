using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_searching_for_text_with_match_whole_word_option : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticTextAndMoveIfNeeded(new AddStaticText(
                questionnaire.Id,
                entityId: staticTextId,
                responsibleId:responsibleId,
                text: $"static text {searchFor} title with ",
                parentId: chapterId));
            questionnaire.AddStaticTextAndMoveIfNeeded(new AddStaticText(questionnaire.Id, entityId: staticTextId1,
                text: $"static text  {searchFor}title", responsibleId:responsibleId,
                parentId: chapterId));
            
        };

        Because of = () => matches = questionnaire.FindAllTexts(searchFor.ToLower(), false, true, false);

        It should_find_whole_word_match = () => matches.ShouldContain(x => x.Id == staticTextId);

        It should_not_find_parts_of_word = () => matches.ShouldNotContain(x => x.Id == staticTextId1);

        static Questionnaire questionnaire;

        static Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid staticTextId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Guid staticTextId1 = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static string searchFor = "With Casing";
        static IEnumerable<QuestionnaireNodeReference> matches;
    }
}