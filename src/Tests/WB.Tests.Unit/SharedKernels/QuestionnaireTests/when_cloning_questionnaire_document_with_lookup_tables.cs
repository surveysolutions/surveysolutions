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
    internal class when_cloning_questionnaire_document_with_lookup_tables
    {
        [OneTimeSetUp]
        public void context()
        {
            document = Create.Entity.QuestionnaireDocumentWithOneChapter();
            document.LookupTables = new Dictionary<Guid, LookupTable>
            {
                { Id.g1, Create.Entity.LookupTable("table 1", "file 1") },
                { Id.g2, Create.Entity.LookupTable("table 2", "file 2") },
            };

            clonedDocument = document.Clone();
        }

        [Test]
        public void should_create_2_tables() =>
            clonedDocument.LookupTables.Count.Should().Be(2);

        [Test]
        public void should_clone_all_fields_for_1st_table()
        {
            var table = clonedDocument.LookupTables.ElementAt(0);
            table.Key.Should().Be(Id.g1);
            table.Value.TableName.Should().Be("table 1");
            table.Value.FileName.Should().Be("file 1");
        }

        [Test]
        public void should_clone_1st_table_with_different_reference() =>
            clonedDocument.LookupTables.ElementAt(0).Should().NotBeSameAs(document.LookupTables.ElementAt(0));

        [Test]
        public void should_clone_all_fields_for_2nd_table()
        {
            var table = clonedDocument.LookupTables.ElementAt(1);
            table.Key.Should().Be(Id.g2);
            table.Value.TableName.Should().Be("table 2");
            table.Value.FileName.Should().Be("file 2");
        }

        [Test]
        public void should_clone_2nd_table_with_different_reference() =>
            clonedDocument.LookupTables.ElementAt(1).Should().NotBeSameAs(document.LookupTables.ElementAt(1));

        private static QuestionnaireDocument document;
        private static QuestionnaireDocument clonedDocument;
    }
}
