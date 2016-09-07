using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_question_with_empty_cascading_options : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            rootGroupId = Guid.Parse("00000000000000000000000000000000");
            actorId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            updatedQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
            (
                publicKey : parentQuestionId,
                groupPublicKey:rootGroupId,
                questionType : QuestionType.SingleOption,
                answers : new[] {
                    new Answer
                    {
                        AnswerText = "one",
                        AnswerValue = "1",
                        PublicKey = Guid.NewGuid()
                    }
                }
            ));

            questionnaire.AddQuestion(Create.Event.NewQuestionAdded
            (
                publicKey : updatedQuestionId,
                groupPublicKey: rootGroupId,
                questionType : QuestionType.SingleOption
            ));

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.UpdateSingleOptionQuestion(
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
            new[]
            {
                new Option(Guid.NewGuid(), String.Empty, String.Empty, (decimal?)null), 
                new Option(Guid.NewGuid(), String.Empty, String.Empty, (decimal?)null), 
                new Option(Guid.NewGuid(), String.Empty, String.Empty, (decimal?)null) 
            }, 
            null,
            false,
            cascadeFromQuestionId: parentQuestionId, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties());

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QuestionChanged_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QuestionChanged>().Answers.Count().ShouldEqual(0);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Guid rootGroupId;
        private static Guid updatedQuestionId;
        private static Guid actorId;
    }
}

