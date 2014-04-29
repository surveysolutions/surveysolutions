using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneMultiOptionQuestionHandlerTests
{
    internal class when_cloning_group_and_group_already_exists : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId, GroupText = "Chapter"});
        };

        Because of = () =>
            exception =
                Catch.Exception(
                    () =>
                        questionnaire.CloneGroup(responsibleId: responsibleId, groupId: chapterId,
                            sourceGroupId: chapterId, targetIndex: 0));

        private It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        private It should_throw_exception_with_message_containting__group__already__exist__ = () =>
            new[] { "group", "already", "exist" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        
    }
}