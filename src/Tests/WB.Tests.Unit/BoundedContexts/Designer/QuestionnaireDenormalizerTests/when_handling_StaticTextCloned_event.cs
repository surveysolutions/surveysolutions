using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextCloned_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireEntityFactory = new Mock<IQuestionnaireEntityFactory>();

            questionnaireEntityFactory.Setup(x => x.CreateStaticText(targetEntityId, text))
                .Returns(CreateStaticText(entityId: targetEntityId, text: text));


            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentId,
                    children: new IComposite[]
                    {
                        new NumericQuestion(),
                        CreateStaticText(entityId: sourceEntityId, text: "old text")
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
            denormalizer.Handle(CreateStaticTextClonedEvent(targetEntityId: targetEntityId,
                sourceEntityId: sourceEntityId, parentId: parentId, targetIndex: targetIndex, text: text));

        It should_call_CreateStaticText_in_questionnaireEntityFactory_only_ones = () =>
           questionnaireEntityFactory.Verify(x => x.CreateStaticText(targetEntityId, text), Times.Once);

        It should__static_text_be_in_questionnaire_document_view = () =>
            GetExpectedStaticText().ShouldNotBeNull();

        It should_PublicKey_be_equal_to_entityId = () =>
           GetExpectedStaticText().PublicKey.ShouldEqual(targetEntityId);

        It should_parent_group_exists_in_questionnaire = () =>
           questionnaireView.Find<IGroup>(parentId).ShouldNotBeNull();

        It should_text_be_equal_specified_text = () =>
            GetExpectedStaticText().Text.ShouldEqual(text);

        It should_target_index_of_static_text_in_parent_group_be_equal_to_specified_targetIndex = () =>
           questionnaireView.Find<IGroup>(parentId).Children.IndexOf(GetExpectedStaticText()).ShouldEqual(targetIndex);

        private static IStaticText GetExpectedStaticText()
        {
            return GetEntityById<IStaticText>(questionnaireView, targetEntityId);
        }

        private static Mock<IQuestionnaireEntityFactory> questionnaireEntityFactory;
        private static QuestionnaireDocument questionnaireView;
        private static QuestionnaireDenormalizer denormalizer;
        private static Guid targetEntityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid parentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string text = "some text";
        private static int targetIndex = 2;
    }
}