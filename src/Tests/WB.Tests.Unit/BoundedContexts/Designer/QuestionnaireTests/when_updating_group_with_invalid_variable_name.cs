using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_group_with_invalid_variable_name : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId, VariableName = "valid"});
            questionnaire.Apply(new GroupBecameARoster(responsibleId, rosterId));
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.UpdateGroup(
                groupId: rosterId,
                responsibleId: responsibleId,
                title: "title",
                variableName: "this",
                rosterSizeQuestionId: null,
                description: null,
                condition: null,
                isRoster: true,
                rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterFixedTitles: new [] { new Tuple<string, string>("1","1"), new Tuple<string, string>("2","2") },
                rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting_message_about_csharp_keywords = () =>
            exception.Message.ToLower().ShouldContain("variable name shouldn't match with keywords");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}