using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_deleting_group_containing_parent_cascading_for_question_outside_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId1 = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var chapterId2 = Guid.Parse("BCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var parentQuestionId = Guid.Parse("11111111111111111111111111111111");
            var cascadingQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId1 });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = parentQuestionId,
                QuestionType = QuestionType.SingleOption,
                GroupPublicKey = chapterId1
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId2 });
            
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = cascadingQuestionId,
                QuestionType = QuestionType.SingleOption,
                GroupPublicKey = chapterId2,
                CascadeFromQuestionId = parentQuestionId 
            });
        };

        Because of = () => exception = Catch.Exception(() =>
                questionnaire.DeleteGroup(chapterId1, responsibleId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting_message_about_preconditions = () =>
            exception.Message.ToLower().ShouldContain("sub-section or roster containing question that is used as parent in cascading combo box cannot be removed before all child questions are removed");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid chapterId1;
        private static Guid responsibleId;
    }
}