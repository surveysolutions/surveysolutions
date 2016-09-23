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
    internal class when_pasting_group_into_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupToPasteInId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            sourceQuestionaireId = Guid.Parse("DCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireWithOneGroup(questionnaireId : questionnaireId, groupId: groupToPasteInId, responsibleId: responsibleId);

            
            doc = Create.QuestionnaireDocument(
                Guid.Parse("31111111111111111111111111111113"),
                Create.Chapter(chapterId: chapterId, children: new List<IComposite>
                {
                    Create.NumericIntegerQuestion(id: level1QuestionId, variable: stataExportCaption)
                }));

            command = new PasteInto(
                questionnaireId: questionnaireId,
                entityId: targetId,
                sourceItemId: chapterId,
                responsibleId: responsibleId,
                sourceQuestionnaireId: questionnaireId,
                parentId: groupToPasteInId);

            command.SourceDocument = doc;
        };

        Because of = () => 
            questionnaire.PasteInto(command);

        It should_clone_group = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId).ShouldNotBeNull();

        It should_clone_group_with_QuestionId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(targetId)
                .PublicKey.ShouldEqual(targetId);
        
        static Questionnaire questionnaire;
        static Guid groupToPasteInId;

        static Guid chapterId = Guid.Parse("CCDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid questionnaireId = Guid.Parse("CCDDDDDDDDDDDDDDDDDDDDDDDDDDDDDA");
        static Guid sourceQuestionaireId;
        static Guid level1QuestionId = Guid.Parse("44DDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        static Guid responsibleId;
        static Guid targetId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string stataExportCaption = "varrr";
        private static QuestionnaireDocument doc;
        private static PasteInto command;
    }
}

