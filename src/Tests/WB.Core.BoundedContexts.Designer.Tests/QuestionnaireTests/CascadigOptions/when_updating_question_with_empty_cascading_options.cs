using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests.CascadigOptions
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

            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = parentQuestionId,
                QuestionType = QuestionType.SingleOption,
                Answers = new[] {
                    new Answer
                    {
                        AnswerText = "one",
                        AnswerValue = "1",
                        PublicKey = Guid.NewGuid()
                    }
                }
            });

            questionnaire.Apply(new NewQuestionAdded
            {
                PublicKey = updatedQuestionId,
                QuestionType = QuestionType.SingleOption,
            });

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.UpdateSingleOptionQuestion(
            updatedQuestionId,
            "title",
            "var",
            null,
            false,
            false,
            QuestionScope.Interviewer,
            null,
            null,
            null,
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
            cascadeFromQuestionId: parentQuestionId
            );

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

