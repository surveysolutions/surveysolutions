using JsonDiffPatchDotNet;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDiffTests
{
    public class should_be_able_to_compare_documents
    {
        [Test]
        public void should_diff_when_text_update()
        {
            QuestionnaireDocument document = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Group(Id.g2, children: new IComposite[]
                {
                    Create.TextQuestion(Id.g3)
                }),
                Create.Group(Id.g4, children: new IComposite[]
                {
                    Create.TextQuestion(questionId: Id.g1, variable: "test", text: "Question text before update"),
                }),
            });

            var serializer = new EntitySerializer<QuestionnaireDocument>();

            var jsonBeforeUpdate = serializer.Serialize(document);

            document.UpdateQuestion(Id.g1, question => question.QuestionText = "updated question text");

            var jsonAfterUpdate = serializer.Serialize(document);

            // Act
            var jdp = new JsonDiffPatch();
            string diff = jdp.Diff(jsonBeforeUpdate, jsonAfterUpdate);

            Assert.That(diff, Does.Contain("Question text before update"));
            Assert.That(diff, Does.Contain("updated question text"));
            Assert.That(diff, Does.Not.Contain(Id.g3));
        }

        [Test]
        public void should_be_able_to_revert_changes()
        {
            QuestionnaireDocument document = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Group(Id.g2, children: new IComposite[]
                {
                    Create.TextQuestion(Id.g3)
                }),
                Create.Group(Id.g4, children: new IComposite[]
                {
                    Create.TextQuestion(questionId: Id.g1, variable: "test", text: "Question text before update"),
                }),
            });

            var serializer = new EntitySerializer<QuestionnaireDocument>();

            var jsonBeforeUpdate = serializer.Serialize(document);

            document.UpdateQuestion(Id.g1, question => question.QuestionText = "updated question text");

            var jsonAfterUpdate = serializer.Serialize(document);

            // Act
            var jdp = new JsonDiffPatch();
            string diff = jdp.Diff(jsonBeforeUpdate, jsonAfterUpdate);

            var unpatched = jdp.Unpatch(jsonAfterUpdate, diff);

            // Assert
            var unpatchedDocument = serializer.Deserialize(unpatched);

            Assert.That(unpatchedDocument.GetQuestion<TextQuestion>(Id.g1).QuestionText, Is.EqualTo("Question text before update"));
        }
    }
}
