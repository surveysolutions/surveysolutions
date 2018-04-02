using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using NUnit.Framework;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.QuestionnaireTests
{
    internal class when_cloning_questionnaire_document_with_macros
    {
        [OneTimeSetUp]
        public void context()
        {
            document = Create.Entity.QuestionnaireDocumentWithOneChapter();
            document.Macros = new Dictionary<Guid, Macro>
            {
                { Id.g1, Create.Entity.Macro("macro1", "content 1", "description 1") },
                { Id.g2, Create.Entity.Macro("macro2", "content 2", "description 2") },
            };

            // Act
            clonedDocument = document.Clone();
        }

        [Test]
        public void should_create_2_macros()
        {
            clonedDocument.Macros.Count.Should().Be(2);
        }

        [Test]
        public void should_clone_all_fields_for_1st_macro()
        {
            var macro = clonedDocument.Macros.ElementAt(0);
            macro.Key.Should().Be(Id.g1);
            macro.Value.Name.Should().Be("macro1");
            macro.Value.Content.Should().Be("content 1");
            macro.Value.Description.Should().Be("description 1");
        }

        [Test]
        public void should_clone_1st_macro_with_different_reference() =>
            clonedDocument.Macros.ElementAt(0).Should().NotBeSameAs(document.Macros.ElementAt(0));

        [Test] public void should_clone_all_fields_for_2nd_macro()
        {
            var macro = clonedDocument.Macros.ElementAt(1);
            macro.Key.Should().Be(Id.g2);
            macro.Value.Name.Should().Be("macro2");
            macro.Value.Content.Should().Be("content 2");
            macro.Value.Description.Should().Be("description 2");
        }

        [Test]
        public void should_clone_2nd_macro_with_different_reference() =>
            clonedDocument.Macros.ElementAt(1).Should().NotBeSameAs(document.Macros.ElementAt(1));

        private static QuestionnaireDocument document;
        private static QuestionnaireDocument clonedDocument;
    }
}
