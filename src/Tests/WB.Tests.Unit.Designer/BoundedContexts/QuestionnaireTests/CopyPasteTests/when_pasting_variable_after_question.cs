using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CopyPasteTests
{
    internal class when_pasting_variable_after_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionToPastAfterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCE");
            questionnaire = CreateQuestionnaireWithOneQuestion(questionToPastAfterId, responsibleId, questionnaireId);
            
            doc = Create.QuestionnaireDocument(Guid.Parse("31111111111111111111111111111113"),
                Create.Chapter(children: new List<IComposite>
                {
                    Create.Variable(variableId, variableType, variableName, variableExpression)
                }));

            eventContext = new EventContext();

            command = new PasteAfter(
               questionnaireId: questionnaireId,
               entityId: targetId,
               sourceItemId: variableId,
               responsibleId: responsibleId,
               sourceQuestionnaireId: questionnaireId,
               itemToPasteAfterId: questionToPastAfterId);

            command.SourceDocument = doc;
        };

        Because of = () => 
            questionnaire.PasteAfter(command);

        It should_raise_VariableCloned_event =
            () => eventContext.ShouldContainEvent<VariableCloned>();

        It should_raise_VariableCloned_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<VariableCloned>().EntityId.ShouldEqual(targetId);

        It should_raise_VariableCloned_event_with_variableType_specified = () =>
            eventContext.GetSingleEvent<VariableCloned>().VariableData.Type.ShouldEqual(variableType);

        It should_raise_VariableCloned_event_with_variableName_specified = () =>
            eventContext.GetSingleEvent<VariableCloned>().VariableData.Name.ShouldEqual(variableName);

        It should_raise_VariableCloned_event_with_variableExpression_specified = () =>
            eventContext.GetSingleEvent<VariableCloned>().VariableData.Expression.ShouldEqual(variableExpression);

        static Questionnaire questionnaire;
        static Guid questionToPastAfterId;

        static Guid variableId = Guid.Parse("44DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static EventContext eventContext;
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static VariableType variableType = VariableType.Integer;
        static string variableName = "name";
        static string variableExpression = "Expression";
        static QuestionnaireDocument doc;

        private static PasteAfter command;
    }
}

