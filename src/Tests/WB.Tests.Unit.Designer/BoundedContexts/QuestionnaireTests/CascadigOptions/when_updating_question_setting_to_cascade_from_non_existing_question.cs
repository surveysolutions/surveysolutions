using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_question_setting_to_cascade_from_non_existing_question : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rootGroupId = Guid.NewGuid();
            actorId = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            parentQuestionId = Guid.NewGuid();
            updatedQuestionId = Guid.NewGuid();

            questionnaire.AddSingleOptionQuestion(parentQuestionId,
                rootGroupId,
                responsibleId:actorId);
            questionnaire.AddSingleOptionQuestion(
                updatedQuestionId,
                rootGroupId,
                actorId);
            BecauseOf();
        }

        private void BecauseOf() => exception = Catch.Exception(() => questionnaire.UpdateSingleOptionQuestion(updatedQuestionId, 
            "title",
            "varia",
            null,
            false,
            QuestionScope.Interviewer,
            null,
            false,
            null,
            actorId,
            new Option[]{}, 
            null,
            false,
            cascadeFromQuestionId: Guid.NewGuid(), validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties()));

        [NUnit.Framework.Test] public void should_not_allow_cascades_from_non_existing_question () 
        {
            var ex = exception as QuestionnaireException;
            ex.ShouldNotBeNull();
            new[] { "cascade", "should", "exist" }.ShouldEachConformTo(keyword => ex.Message.ToLower().Contains(keyword));
        }
        private static Guid rootGroupId;
        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Guid updatedQuestionId;
        private static Exception exception;
        private static Guid actorId;
    }
}

