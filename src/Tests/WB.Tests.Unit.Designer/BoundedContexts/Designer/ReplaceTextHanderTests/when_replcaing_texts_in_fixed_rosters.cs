using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replcaing_texts_in_fixed_rosters : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
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

        Because of = () => questionnaire.ReplaceTexts(Create.Command.ReplaceTextsCommand(searchFor, replaceWith, userId: responsibleId));

        It should_replace_fixed_roster_title = () =>
                questionnaire.QuestionnaireDocument.Find<IGroup>(rosterId).FixedRosterTitles[1].Title.ShouldEqual($"two with {replaceWith}");

        It should_record_number_of_replaced_entities = () => questionnaire.GetLastReplacedEntriesCount().ShouldEqual(1);

        static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static Questionnaire questionnaire;

        static readonly Guid rosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
        private static Guid responsibleId;
        const string searchFor = "to_replace";
        const string replaceWith = "replaced";
    }
}