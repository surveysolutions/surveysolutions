using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.CloneStaticTextTests
{
    public class when_cloning_StaticText_without_permissions_to_edit_questionnaire : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            sourceStaticTextId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var sourceParentId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var sourceText = "source text";

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, groupId: sourceParentId);
            questionnaire.Apply(Create.Event.StaticTextAdded(parentId: sourceParentId,
                                                             responsibleId: responsibleId,
                                                             publicKey: sourceStaticTextId,
                                                             text: sourceText));

            eventContext = new EventContext();
        };

        Because of = () => exception = Catch.Exception(() => questionnaire.CloneStaticText(Guid.NewGuid(), sourceStaticTextId, Guid.NewGuid()));

        It should_thow_exception_with_DoesNotHavePermissionsForEdit_type = () => {
            var questionnaireException = exception as QuestionnaireException;
            questionnaireException.ShouldNotBeNull();
            questionnaireException.ErrorType.ShouldEqual(DomainExceptionType.DoesNotHavePermissionsForEdit);
        };

        static Questionnaire questionnaire;
        static EventContext eventContext;
        static Guid sourceStaticTextId;
        static Exception exception;
    }
}

