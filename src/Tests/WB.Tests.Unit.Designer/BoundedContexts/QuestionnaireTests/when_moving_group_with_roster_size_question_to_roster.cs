using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_group_with_roster_size_question_to_roster : QuestionnaireTestsContext
    {
         Establish context = () =>
         {
             questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
             questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
             questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
             questionnaire.AddQuestion(Create.Event.NumericQuestionAdded(
                 publicKey: rosterSizeQuestionId,
                  groupPublicKey: groupId,
                  isInteger: true
             ));
             questionnaire.AddGroup(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId });
             questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, rosterId));
             questionnaire.ChangeRoster(new RosterChanged(responsibleId: responsibleId, groupId: rosterId)
             {
                 RosterSizeQuestionId = rosterSizeQuestionId,
                 RosterSizeSource = RosterSizeSourceType.Question,
                 FixedRosterTitles = null,
                 RosterTitleQuestionId = null
             });
         };

        Because of = () =>
             exception = Catch.Exception(() =>
                 questionnaire.MoveGroup(groupId, rosterId, 0, responsibleId));

        It should_throw_QuestionnaireException = () =>
             exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__contains__ = () =>
             exception.Message.ToLower().ShouldContain("contains");

        It should_throw_exception_with_message_containting__roster__ = () =>
             exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__size__ = () =>
             exception.Message.ToLower().ShouldContain("source");

        It should_throw_exception_with_message_containting__question__ = () =>
             exception.Message.ToLower().ShouldContain("question");

        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Exception exception;
    }
}