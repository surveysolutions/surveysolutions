using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_that_is_linked_in_roster_group_that_used_as_roster_title_question : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = anotherRosterId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, anotherRosterId));
            questionnaire.Apply(new NumericQuestionAdded
            {
                PublicKey = rosterSizeQuestionId,
                IsInteger = true,
                GroupPublicKey = anotherRosterId
            });
            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = rosterTitleQuestionId,
                GroupPublicKey = anotherRosterId,
                QuestionType = QuestionType.MultyOption
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId });
            questionnaire.Apply(new GroupBecameARoster(responsibleId, groupId));
            questionnaire.Apply(new RosterChanged(responsibleId, groupId){
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    FixedRosterTitles =  null,
                    RosterTitleQuestionId =rosterTitleQuestionId 
                });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateMultiOptionQuestion(
                questionId: rosterTitleQuestionId,
                title: title,
                variableName: variableName,
                variableLabel: null,
                isMandatory: isMandatory,
                scope: scope,
                enablementCondition: enablementCondition,
                validationExpression: validationExpression,
                validationMessage: validationMessage,
                instructions: instructions,
                responsibleId: responsibleId,
                options: options,
                linkedToQuestionId: linkedToQuestionId,
                areAnswersOrdered: areAnswersOrdered,
                maxAllowedAnswers: maxAllowedAnswers
                ));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__categorical_could_not_be_roster_title_question__ = () =>
           new[] { "linked categorical multy answers question could not be used as a roster title question in group(s)" }.ShouldEachConformTo(
               keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "some condition";
        private static string validationExpression = "some validation";
        private static string validationMessage = "validation message";
        private static Option[] options = null;
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
    }
}