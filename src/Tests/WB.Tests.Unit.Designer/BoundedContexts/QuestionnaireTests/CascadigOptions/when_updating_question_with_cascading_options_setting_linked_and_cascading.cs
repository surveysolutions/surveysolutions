using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_question_with_cascading_options_setting_linked_and_cascading : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rootGroupId = Guid.NewGuid();
            actorId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            parentQuestionId = Guid.NewGuid();
            updatedQuestionId = Guid.NewGuid();

            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                rootGroupId,
                responsibleId:actorId,
                options : new Option[] {
                    new Option{Title = "one",Value = "1",Id = Guid.NewGuid()},
                    new Option{Title = "two",Value = "2",Id = Guid.NewGuid()}
                }
            );
            questionnaire.AddSingleOptionQuestion(
                 updatedQuestionId,
                rootGroupId,
                responsibleId:actorId,
                options: new Option[] {
                    new Option{Title = "one",Value = "1",Id = Guid.NewGuid()},
                    new Option{Title = "two",Value = "2",Id = Guid.NewGuid()}
                });
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.UpdateSingleOptionQuestion(
            updatedQuestionId,
            "title",
            "var",
            null,
            false,
            QuestionScope.Interviewer,
            null,
            false,
            null,
            actorId,
            new Option[]{}, 
            parentQuestionId,
            false,
            cascadeFromQuestionId: parentQuestionId, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
            linkedFilterExpression: null, properties: Create.QuestionProperties()));

        It should_not_allow_to_set_both_linked_and_cascading_qestion_at_the_same_time = () =>
        {
            var ex = exception as QuestionnaireException;
            ex.ShouldNotBeNull();

            new []{"cascading", "linked", "same", "time"}.ShouldEachConformTo(keyword => ex.Message.ToLower().Contains(keyword));
        };


        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Exception exception;
        private static Guid rootGroupId;
        private static Guid updatedQuestionId;
        private static Guid actorId;
    }
}

