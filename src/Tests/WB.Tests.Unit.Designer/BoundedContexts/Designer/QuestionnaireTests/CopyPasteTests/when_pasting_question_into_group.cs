using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests.CopyPasteTests
{
    internal class when_pasting_question_into_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupToPasteInId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            sourceQuestionaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var questionnaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCE");
            questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId : questionnaireId, groupId: groupToPasteInId, responsibleId: responsibleId);

            
            doc = Create.QuestionnaireDocument(
                Guid.Parse("31111111111111111111111111111113"),
                Create.Chapter(children: new List<IComposite>
                {
                    Create.NumericIntegerQuestion(id: level1QuestionId, variable: stataExportCaption)
                    
                }));
            eventContext = new EventContext();

            command = new PasteInto(
                questionnaireId: questionnaireId,
                entityId: targetId,
                sourceItemId: level1QuestionId,
                responsibleId: responsibleId,
                sourceQuestionnaireId: questionnaireId,
                parentId: groupToPasteInId);

            command.SourceDocument = doc;
        };

        Because of = () => 
            questionnaire.PasteInto(command);

        private It should_clone_MaxAnswerCount_value =
            () => eventContext.ShouldContainEvent<QuestionCloned>();

        It should_raise_QuestionCloned_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .PublicKey.ShouldEqual(targetId);

        It should_raise_QuestionCloned_event_with_stataExportCaption_specified = () =>
            eventContext.GetSingleEvent<QuestionCloned>()
                .StataExportCaption.ShouldEqual(stataExportCaption);

        static Questionnaire questionnaire;
        static Guid groupToPasteInId;

        static Guid sourceQuestionaireId;
        static Guid level1QuestionId = Guid.Parse("44DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static EventContext eventContext;
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string stataExportCaption = "varrr";
        private static QuestionnaireDocument doc;

        private static PasteInto command;
    }
}

