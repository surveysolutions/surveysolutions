using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_StaticTextUpdated_event : QuestionnaireDenormalizerTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireView = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentId,
                    children: new IComposite[]
                    {
                        Create.StaticText(staticTextId: entityId, text: "old text")
                    })
            }, creatorId);

            denormalizer = CreateQuestionnaireDenormalizer(questionnaire: questionnaireView);
            BecauseOf();
        }

        private void BecauseOf() =>
            denormalizer.UpdateStaticText(new UpdateStaticText(questionnaireView.PublicKey, entityId, text, attachment, creatorId, null, false, new List<ValidationCondition>()));
        
        [NUnit.Framework.Test] public void should__static_text_be_in_questionnaire_document_view () =>
            GetExpectedStaticText().ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_PublicKey_be_equal_to_entityId () =>
           GetExpectedStaticText().PublicKey.ShouldEqual(entityId);

        [NUnit.Framework.Test] public void should_parent_group_exists_in_questionnaire () =>
           questionnaireView.Find<IGroup>(parentId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_parent_group_contains_static_text () =>
           questionnaireView.Find<IGroup>(parentId).Children[0].PublicKey.ShouldEqual(entityId);

        [NUnit.Framework.Test] public void should_text_be_equal_specified_text () =>
            GetExpectedStaticText().Text.ShouldEqual(text);

        [NUnit.Framework.Test] public void should_AttachmentName_be_equal_specified_attachment () =>
            GetExpectedStaticText().AttachmentName.ShouldEqual(attachment);

        private static IStaticText GetExpectedStaticText()
        {
            return GetEntityById<IStaticText>(document: questionnaireView, entityId: entityId);
        }

        private static QuestionnaireDocument questionnaireView;
        private static Questionnaire denormalizer;
        private static Guid entityId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid creatorId = Guid.Parse("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string text = "some text";
        private static string attachment = "bananas";
    }
}