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

        It should_contains_variable = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(targetId).ShouldNotBeNull();

        It should_contains_variable_with_QuestionId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(targetId).PublicKey.ShouldEqual(targetId);

        It should_contains_variable_with_variableType_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(targetId).Type.ShouldEqual(variableType);

        It should_contains_variable_with_variableName_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(targetId).Name.ShouldEqual(variableName);

        It should_contains_variable_with_variableExpression_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IVariable>(targetId).Expression.ShouldEqual(variableExpression);

        static Questionnaire questionnaire;
        static Guid questionToPastAfterId;

        static Guid variableId = Guid.Parse("44DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static VariableType variableType = VariableType.LongInteger;
        static string variableName = "name";
        static string variableExpression = "Expression";
        static QuestionnaireDocument doc;

        private static PasteAfter command;
    }
}

