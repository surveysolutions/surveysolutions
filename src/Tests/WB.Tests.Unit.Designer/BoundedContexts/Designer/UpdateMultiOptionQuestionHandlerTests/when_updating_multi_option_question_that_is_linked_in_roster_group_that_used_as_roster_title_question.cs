using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_that_is_linked_in_roster_group_that_used_as_roster_title_question : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = anotherRosterId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, anotherRosterId));
            questionnaire.AddNumericQuestion(rosterSizeQuestionId, anotherRosterId, responsibleId, isInteger : true);

            questionnaire.AddMultiOptionQuestion(rosterTitleQuestionId,anotherRosterId,responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = groupId });
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, groupId));
            questionnaire.ChangeRoster(new RosterChanged(responsibleId, groupId){
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
                scope: scope,
                enablementCondition: enablementCondition,
                hideIfDisabled: false,
                instructions: instructions,
                responsibleId: responsibleId,
                options: options,
                linkedToEntityId: linkedToQuestionId,
                areAnswersOrdered: areAnswersOrdered,
                maxAllowedAnswers: maxAllowedAnswers,
                yesNoView: yesNoView, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties()));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__categorical_could_not_be_roster_title_question__ = () =>
           new[] { "linked categorical multi-select question could not be used as a roster title question in sub-section(s)" }.ShouldEachConformTo(
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
        private static string title = "title";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "some condition";
        private static Option[] options = null;
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
        private static bool yesNoView = false;
    }
}