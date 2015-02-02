using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireEntityFactory = new Mock<IQuestionnaireEntityFactory>();

            questionnaireEntityFactory.Setup(x => x.CreateStaticText(entityId, text))
                .Returns(CreateStaticText(entityId: entityId, text: text));

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentId,
                    children: new IComposite[]
                    {
                        CreateStaticText(entityId: entityId, text: "old text")
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

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage.Object, questionnaireEntityFactory: questionnaireEntityFactory.Object);
        };

        Because of = () =>
            denormalizer.Handle(CreateStaticTextUpdatedEvent(entityId: entityId, text: text));

        It should_call_CreateStaticText_in_questionnaireEntityFactory_only_ones = () =>
            questionnaireEntityFactory.Verify(x => x.CreateStaticText(entityId, text), Times.Once);

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

        private static IStaticText GetExpectedStaticText()
        {
            return GetEntityById<IStaticText>(document: questionnaireView, entityId: entityId);
        }

        private static Mock<IQuestionnaireEntityFactory> questionnaireEntityFactory;
        private static QuestionnaireDocument questionnaireView;
        private static QuestionnaireDenormalizer denormalizer;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string text = "some text";
    }
}