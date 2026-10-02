using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer
{
    [TestFixture]
    [TestOf(typeof(QuestionnaireEntityReferenceExtension))]
    public class QuestionnaireEntityReferenceExtensionTests
    {
        private static ReadOnlyQuestionnaireDocument Wrap(QuestionnaireDocument doc)
            => new ReadOnlyQuestionnaireDocument(doc);

        [Test]
        public void ExtendedReference_for_questionnaire_should_use_document_title_and_variable()
        {
            var id = Guid.NewGuid();
            var doc = Create.QuestionnaireDocument(id: id, title: "My title", children: new IComposite[0]);
            doc.VariableName = "my_var";

            var result = new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Questionnaire, id)
                .ExtendedReference(Wrap(doc));

            result.Type.Should().Be(QuestionnaireVerificationReferenceType.Questionnaire);
            result.ItemId.Should().Be(id.FormatGuid());
            result.Title.Should().Be("My title");
            result.Variable.Should().Be("my_var");
        }

        [Test]
        public void ExtendedReference_for_question_should_resolve_question_details()
        {
            var questionId = Guid.NewGuid();
            var doc = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Group(groupId: Id.g1, children: new IComposite[]
                {
                    Create.TextQuestion(questionId, text: "What is your name?", variable: "name")
                })
            });
            var reference = new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Question, questionId)
            {
                IndexOfEntityInProperty = 3
            };

            var result = reference.ExtendedReference(Wrap(doc));

            result.Type.Should().Be(QuestionnaireVerificationReferenceType.Question);
            result.Variable.Should().Be("name");
            result.Title.Should().Be("What is your name?");
            result.QuestionType.Should().Be("icon-text");
            result.IndexOfEntityInProperty.Should().Be(3);
            result.ChapterId.Should().NotBeNull();
        }

        [Test]
        public void ExtendedReference_for_missing_question_should_fall_back_to_empty_title()
        {
            var doc = Create.QuestionnaireDocument(children: new IComposite[0]);

            var result = new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Question, Guid.NewGuid())
                .ExtendedReference(Wrap(doc));

            result.Title.Should().BeEmpty();
            result.Variable.Should().BeNull();
        }

        [Test]
        public void ExtendedReference_for_group_should_resolve_group_details()
        {
            var groupId = Guid.NewGuid();
            var doc = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Group(groupId: groupId, title: "Group title", variable: "grp")
            });

            var result = new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Group, groupId)
                .ExtendedReference(Wrap(doc));

            result.Type.Should().Be(QuestionnaireVerificationReferenceType.Group);
            result.Title.Should().Be("Group title");
            result.Variable.Should().Be("grp");
        }

        [Test]
        public void ExtendedReference_for_attachment_not_in_document_should_throw()
        {
            var doc = Create.QuestionnaireDocument(children: new IComposite[0]);
            var reference = new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Attachment, Guid.NewGuid());

            Action act = () => reference.ExtendedReference(Wrap(doc));

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void ExtendedReference_for_macro_not_in_document_should_throw()
        {
            var doc = Create.QuestionnaireDocument(children: new IComposite[0]);
            var reference = new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Macro, Guid.NewGuid());

            Action act = () => reference.ExtendedReference(Wrap(doc));

            act.Should().Throw<InvalidOperationException>();
        }
    }
}

