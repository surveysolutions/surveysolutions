using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireEntityFactory = Setup.QuestionnaireEntityFactoryWithStaticText(entityId, text, attachment);

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentId,
                    children: new IComposite[]
                    {
                        Create.StaticText(staticTextId: entityId, text: "old text")
                    })
            });

            var documentStorage = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();
            
            documentStorage
                .Setup(writer => writer.GetById(Moq.It.IsAny<string>()))
                .Returns(questionnaireDocument);
            
            documentStorage
                .Setup(writer => writer.Store(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<string>()))
                .Callback((QuestionnaireDocument document, string id) =>
                {
                    questionnaireView = document;
                });

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireDocument, questionnaireEntityFactory: questionnaireEntityFactory.Object);
        };

        Because of = () =>
            denormalizer.UpdateStaticText(CreateStaticTextUpdatedEvent(entityId: entityId, text: text, attachmentName: attachment).Payload);

        It should_call_CreateStaticText_in_questionnaireEntityFactory_only_ones = () =>
            questionnaireEntityFactory.Verify(x => x.CreateStaticText(entityId, text, attachment, null, false, Moq.It.IsAny<List<ValidationCondition>>()), Times.Once);

        It should__static_text_be_in_questionnaire_document_view = () =>
            GetExpectedStaticText().ShouldNotBeNull();

        It should_PublicKey_be_equal_to_entityId = () =>
           GetExpectedStaticText().PublicKey.ShouldEqual(entityId);

        It should_parent_group_exists_in_questionnaire = () =>
           questionnaireView.Find<IGroup>(parentId).ShouldNotBeNull();

        It should_parent_group_contains_static_text = () =>
           questionnaireView.Find<IGroup>(parentId).Children[0].PublicKey.ShouldEqual(entityId);

        It should_text_be_equal_specified_text = () =>
            GetExpectedStaticText().Text.ShouldEqual(text);

        It should_AttachmentName_be_equal_specified_attachment = () =>
            GetExpectedStaticText().AttachmentName.ShouldEqual(attachment);

        private static IStaticText GetExpectedStaticText()
        {
            return GetEntityById<IStaticText>(document: questionnaireView, entityId: entityId);
        }

        private static Mock<IQuestionnaireEntityFactory> questionnaireEntityFactory;
        private static QuestionnaireDocument questionnaireView;
        private static Questionnaire denormalizer;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string text = "some text";
        private static string attachment = "bananas";
    }
}