using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.Clone
{
    internal class when_cloning_cloned_numeric_integer_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionnaire = CreateQuestionnaire(responsibleId);
            var groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId });
            var sourceQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCD");

            var questionCloned = CreateQuestionCloned(
                publicKey : questionId,
                sourceQuestionId: sourceQuestionId,
                targetIndex:0,
                questionText : "text",
                questionType : QuestionType.Numeric,
                stataExportCaption : "varrr",
                variableLabel : "varlabel",
                isInteger : true,
                countOfDecimalPlaces : 100
            );
            questionnaire.Apply(questionCloned);

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneQuestionById(questionId, responsibleId, targetId);

        It should_clone_Reset_MaxValue_property = () => eventContext.ShouldContainEvent<QuestionCloned>(x => x.PublicKey == targetId);

        It should_clone_CountOfDecimalPlaces_property = () =>  eventContext.ShouldContainEvent<QuestionCloned>(x => x.PublicKey == targetId && x.CountOfDecimalPlaces == 100);

        static Questionnaire questionnaire;
        static Guid questionId;
        static EventContext eventContext;
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}

