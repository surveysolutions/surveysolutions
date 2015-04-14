using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_deleting_question_and_question_is_referenced_as_roster_title_question_in_roster_triggered_by_multy_option_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            rosterTitleQuestionId = Guid.Parse("21111111111111111111111111111111");
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterTitle = "Roster Title";

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                QuestionType = QuestionType.MultyOption,
                GroupPublicKey = chapterId
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, GroupText = rosterTitle });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.Apply(new RosterChanged(responsibleId, rosterId)
                {
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = rosterSizeQuestionId
                });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterTitleQuestionId,
                QuestionType = QuestionType.Text,
                GroupPublicKey = rosterId
            });
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
                questionnaire.DeleteQuestion(rosterTitleQuestionId, responsibleId);

        It should_raise_QuestionDeleted_event = () =>
          eventContext.ShouldContainEvent<QuestionDeleted>();

        It should_raise_QuestionDeleted_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QuestionDeleted>()
                .QuestionId.ShouldEqual(rosterTitleQuestionId);

        private static string rosterTitle;
        private static Questionnaire questionnaire;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static Guid responsibleId;
        private static EventContext eventContext;
    }
}
