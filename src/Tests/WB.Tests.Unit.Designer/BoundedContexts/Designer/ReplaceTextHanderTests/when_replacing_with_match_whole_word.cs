using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.ReplaceTextHanderTests
{
    internal class when_replacing_with_match_whole_word : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            var responsibleId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId: responsibleId,
                groupId: chapterId);

            questionnaire.AddStaticText(Create.Event.StaticTextAdded(entityId: staticTextId,
                text: $"static {searchFor} title {searchFor}second",
                parentId: chapterId));

            command = Create.Command.ReplaceTextsCommand(searchFor.ToLower(), replaceWith, matchWholeWord: true);
        };

        Because of = () => questionnaire.ReplaceTexts(command);

        It should_find_whole_word_match = () => 
            questionnaire.QuestionnaireDocument.Find<StaticText>(staticTextId).GetTitle().ShouldEqual($"static {replaceWith} title {searchFor}second");

        static Questionnaire questionnaire;

        static Guid chapterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid staticTextId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static string searchFor = "With Casing";
        static ReplaceTextsCommand command;
        const string replaceWith = "replaCed";
    }
}