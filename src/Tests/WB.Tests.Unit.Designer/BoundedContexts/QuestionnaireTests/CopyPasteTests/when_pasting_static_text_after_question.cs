using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CopyPasteTests
{
    internal class when_pasting_static_text_after_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionToPastAfterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            sourceQuestionaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var questionnaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCE");
            questionnaire = CreateQuestionnaireWithOneQuestion(questionToPastAfterId, responsibleId, questionnaireId);

            
            doc = Create.QuestionnaireDocument(
                Guid.Parse("31111111111111111111111111111113"),
                Create.Chapter(children: new List<IComposite>
                {
                    Create.StaticText(staticTextId: staticTextId, text:text)
                    
                }));

            command = new PasteAfter(
               questionnaireId: questionnaireId,
               entityId: targetId,
               sourceItemId: staticTextId,
               responsibleId: responsibleId,
               sourceQuestionnaireId: questionnaireId,
               itemToPasteAfterId: questionToPastAfterId);

            command.SourceDocument = doc;
        };

        Because of = () => 
            questionnaire.PasteAfter(command);

        It should_clone_MaxAnswerCount_value = () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(targetId).ShouldNotBeNull();

        It should_raise_QuestionCloned_event_with_PublicKey_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(targetId)
                .PublicKey.ShouldEqual(targetId);

        It should_raise_QuestionCloned_event_with_stataExportCaption_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IStaticText>(targetId)
                .Text.ShouldEqual(text);

        static Questionnaire questionnaire;
        static Guid questionToPastAfterId;

        static Guid sourceQuestionaireId;
        static Guid staticTextId = Guid.Parse("44DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string text = "varrr";
        private static QuestionnaireDocument doc;

        private static PasteAfter command;
    }
}

