using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.CloneStaticTextTests
{
    public class when_cloning_StaticText_and_source_not_present : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var sourceStaticTextId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var sourceParentId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            const string sourceText = "source text";

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: sourceParentId);
            questionnaire.Apply(Create.Event.StaticTextAdded(parentId: sourceParentId,
                                                             responsibleId: responsibleId,
                                                             publicKey: sourceStaticTextId,
                                                             text: sourceText));

            eventContext = new EventContext();
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.CloneStaticText(Guid.NewGuid(), Guid.NewGuid(), responsibleId));

        It should_thow_exception_with_EntityNotFound_type = () => {
            var questionnaireException = exception as QuestionnaireException;
            questionnaireException.ShouldNotBeNull();
            questionnaireException.ErrorType.ShouldEqual(DomainExceptionType.EntityNotFound);
        };

        static Questionnaire questionnaire;
        static EventContext eventContext;
        static Exception exception;
        static Guid responsibleId;
    };
}


