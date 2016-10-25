using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_searching_for_text_in_fixed_rosters : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddGroup(groupId: rosterId,
                parentGroupId: chapterId,
                responsibleId: responsibleId,
                isRoster: true,
                rosterFixedTitles: new[]
                {
                    new FixedRosterTitleItem("1", "one"),
                    new FixedRosterTitleItem("2", $"two with {searchFor}")
                });
        };

        Because of = () => foundReferences = questionnaire.FindAllTexts(searchFor, false, false, false);

        It should_find_fixed_roster_items = () =>
            foundReferences.ShouldContain(x => x.Id == rosterId && 
                                                  x.Property == QuestionnaireVerificationReferenceProperty.FixedRosterItem &&
                                                  x.IndexOfEntityInProperty == 1);

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid rosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        const string searchFor = "to_replace";

        static IEnumerable<QuestionnaireNodeReference> foundReferences;
    }
}