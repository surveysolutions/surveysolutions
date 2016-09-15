using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CopyPasteTests
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

        It should_exists_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(targetId).ShouldNotBeNull();

        It should_exists_question_with_QuestionId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(targetId)
                .PublicKey.ShouldEqual(targetId);

        It should_exists_question_with_stataExportCaption_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(targetId)
                .StataExportCaption.ShouldEqual(stataExportCaption);

        static Questionnaire questionnaire;
        static Guid groupToPasteInId;

        static Guid sourceQuestionaireId;
        static Guid level1QuestionId = Guid.Parse("44DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string stataExportCaption = "varrr";
        private static QuestionnaireDocument doc;

        private static PasteInto command;
    }
}

