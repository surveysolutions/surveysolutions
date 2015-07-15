using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.Clone
{
    public class when_cloning_cloned_numeric_integer_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = CreateQuestionnaire(responsibleId);
            var groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId });
            sourceQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionCloned = new QuestionCloned
            {
                PublicKey = sourceQuestionId,
                QuestionText = "text",
                QuestionType = QuestionType.Numeric,
                StataExportCaption = "varrr",
                VariableLabel = "varlabel",
                IsInteger = true,
                CountOfDecimalPlaces = 100
            };
            questionnaire.Apply(questionCloned);

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneQuestionById(sourceQuestionId, responsibleId, targetId);

        It should_clone_Reset_MaxValue_property = () => eventContext.ShouldContainEvent<QuestionCloned>(x => x.PublicKey == targetId);

        It should_clone_CountOfDecimalPlaces_property = () =>  eventContext.ShouldContainEvent<QuestionCloned>(x => x.PublicKey == targetId && x.CountOfDecimalPlaces == 100);

        static Questionnaire questionnaire;
        static Guid sourceQuestionId;
        static EventContext eventContext;
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}

