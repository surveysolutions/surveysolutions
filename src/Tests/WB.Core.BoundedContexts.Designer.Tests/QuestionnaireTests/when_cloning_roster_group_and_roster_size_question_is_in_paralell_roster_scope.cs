using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_cloning_roster_group_and_roster_size_question_is_in_paralell_roster_scope : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            cloningRosterGroupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            var parallelRosterId = Guid.Parse("21111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });

            questionnaire.Apply(new NewGroupAdded { PublicKey = anotherRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, anotherRosterId));

            questionnaire.Apply(new NewGroupAdded { PublicKey = parallelRosterId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, parallelRosterId));
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                QuestionType = QuestionType.MultyOption,
                GroupPublicKey = parallelRosterId
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.CloneGroupWithoutChildren(cloningRosterGroupId, responsibleId, "title", null, rosterSizeQuestionId, null, null, anotherRosterId,
                    anotherRosterId, 0, isRoster: true, rosterSizeSource: RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__question_placed_deeper_then_roster = () =>
            new[] { "roster", "question", "deeper" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterSizeQuestionId;
        private static Guid cloningRosterGroupId;
        private static Guid chapterId;
        private static Guid anotherRosterId;
    }
}
