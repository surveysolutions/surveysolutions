using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using System.Collections.Generic;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests.Clone
{
    internal class when_pasting_static_text_after_question : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionToPastAfterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            sourceQuestionaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireWithOneQuestion(questionToPastAfterId, responsibleId);

            
            doc = Create.QuestionnaireDocument(
                Guid.Parse("31111111111111111111111111111113"),
                Create.Chapter(children: new List<IComposite>
                {
                    Create.StaticText(staticTextId: staticTextId, text:text)
                    
                }));
            eventContext = new EventContext();
        };

        Because of = () => 
            questionnaire.PasteItemAfter(targetId, questionToPastAfterId, responsibleId, staticTextId, doc);

        private It should_clone_MaxAnswerCount_value =
            () => eventContext.ShouldContainEvent<StaticTextCloned>();

        It should_raise_QuestionCloned_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<StaticTextCloned>()
                .EntityId.ShouldEqual(targetId);

        It should_raise_QuestionCloned_event_with_stataExportCaption_specified = () =>
            eventContext.GetSingleEvent<StaticTextCloned>()
                .Text.ShouldEqual(text);

        static Questionnaire questionnaire;
        static Guid questionToPastAfterId;

        static Guid sourceQuestionaireId;
        static Guid staticTextId = Guid.Parse("44DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static EventContext eventContext;
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string text = "varrr";
        private static QuestionnaireDocument doc;
    }
}

