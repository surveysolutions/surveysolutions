using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.QuestionnaireTests
{
    internal class when_cloning_questionnaire_document_with_lookup_tables
    {
        Establish context = () =>
        {
            document = Create.QuestionnaireDocumentWithOneChapter();
            document.LookupTables = new Dictionary<Guid, LookupTable>
            {
                { Id.g1, Create.LookupTable("table 1", "file 1") },
                { Id.g2, Create.LookupTable("table 2", "file 2") },
            };
        };

        Because of = () =>
            clonedDocument = document.Clone();

        It should_create_2_tables = () =>
            clonedDocument.LookupTables.Count.ShouldEqual(2);

        It should_clone_all_fields_for_1st_table = () =>
        {
            var table = clonedDocument.LookupTables.ElementAt(0);
            table.Key.ShouldEqual(Id.g1);
            table.Value.TableName.ShouldEqual("table 1");
            table.Value.FileName.ShouldEqual("file 1");
        };

        It should_clone_1st_table_with_different_reference = () =>
            clonedDocument.LookupTables.ElementAt(0).ShouldNotBeTheSameAs(document.LookupTables.ElementAt(0));

        It should_clone_all_fields_for_2nd_table = () =>
        {
            var table = clonedDocument.LookupTables.ElementAt(1);
            table.Key.ShouldEqual(Id.g2);
            table.Value.TableName.ShouldEqual("table 2");
            table.Value.FileName.ShouldEqual("file 2");
        };

        It should_clone_2nd_table_with_different_reference = () =>
            clonedDocument.LookupTables.ElementAt(1).ShouldNotBeTheSameAs(document.LookupTables.ElementAt(1));

        private static QuestionnaireDocument document;
        private static QuestionnaireDocument clonedDocument;
    }
}