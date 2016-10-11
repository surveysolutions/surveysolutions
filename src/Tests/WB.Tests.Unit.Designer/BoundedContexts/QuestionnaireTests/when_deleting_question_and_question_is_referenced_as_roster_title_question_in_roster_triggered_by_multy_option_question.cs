using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
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
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddMultiOptionQuestion(
                rosterSizeQuestionId,
                chapterId,
                responsibleId);
            questionnaire.AddGroup(rosterId, title: rosterTitle, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId, rosterId)
                {
                    RosterSizeQuestionId = null,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles = null,
                    RosterTitleQuestionId = rosterSizeQuestionId
                });
            questionnaire.AddTextQuestion(
                rosterTitleQuestionId,
                rosterId,
                responsibleId);
        };


        Because of = () =>
                questionnaire.DeleteQuestion(rosterTitleQuestionId, responsibleId);

        It should_doesnt_contain_question = () =>
          questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterTitleQuestionId).ShouldBeNull();

        private static string rosterTitle;
        private static Questionnaire questionnaire;
        private static Guid rosterSizeQuestionId;
        private static Guid rosterTitleQuestionId;
        private static Guid responsibleId;
    }
}
