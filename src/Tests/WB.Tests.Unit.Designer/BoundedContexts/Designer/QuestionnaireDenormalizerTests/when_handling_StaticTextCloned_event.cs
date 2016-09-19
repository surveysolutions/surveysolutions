using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextCloned_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionnaireEntityFactory = new Mock<IQuestionnaireEntityFactory>();

            questionnaireEntityFactory.Setup(x => x.CreateStaticText(targetEntityId, text, it.IsAny<string>(), it.IsAny<string>(), it.IsAny<bool>(), Moq.It.IsAny<IList<ValidationCondition>>()))
                .Returns(Create.StaticText(staticTextId: targetEntityId, text: text));


            questionnaireView = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentId,
                    children: new IComposite[]
                    {
                        new NumericQuestion(),
                        Create.StaticText(staticTextId: sourceEntityId, text: "old text")
                    })
            });

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireView, questionnaireEntityFactory: questionnaireEntityFactory.Object);
        };

        Because of = () =>
            denormalizer.CloneStaticText(CreateStaticTextClonedEvent(targetEntityId: targetEntityId,
                sourceEntityId: sourceEntityId, parentId: parentId, targetIndex: targetIndex, text: text).Payload);

        It should_call_CreateStaticText_in_questionnaireEntityFactory_only_ones = () =>
           questionnaireEntityFactory.Verify(x => x.CreateStaticText(targetEntityId, text, it.IsAny<string>(), null, false, Moq.It.IsAny<IList<ValidationCondition>>()), Times.Once);

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
        private static Questionnaire denormalizer;
        private static Guid targetEntityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceEntityId = Guid.Parse("22222222222222222222222222222222");
        private static Guid parentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string text = "some text";
        private static int targetIndex = 2;
    }
}