using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.QuestionnaireTests
{
    internal class when_cloning_questionnaire_document_with_macros
    {
        Establish context = () =>
        {
            document = Create.QuestionnaireDocumentWithOneChapter();
            document.Macros = new Dictionary<Guid, Macro>
            {
                { Id.g1, Create.Macro("macro1", "content 1", "description 1") },
                { Id.g2, Create.Macro("macro2", "content 2", "description 2") },
            };
        };

        Because of = () =>
            clonedDocument = document.Clone();

        It should_create_2_macros = () =>
          clonedDocument.Macros.Count.ShouldEqual(2);

        It should_clone_all_fields_for_1st_macro = () =>
        {
            var macro = clonedDocument.Macros.ElementAt(0);
            macro.Key.ShouldEqual(Id.g1);
            macro.Value.Name.ShouldEqual("macro1");
            macro.Value.Content.ShouldEqual("content 1");
            macro.Value.Description.ShouldEqual("description 1");
        };

        It should_clone_1st_macro_with_different_reference = () =>
            clonedDocument.Macros.ElementAt(0).ShouldNotBeTheSameAs(document.Macros.ElementAt(0));

        It should_clone_all_fields_for_2nd_macro = () =>
        {
            var macro = clonedDocument.Macros.ElementAt(1);
            macro.Key.ShouldEqual(Id.g2);
            macro.Value.Name.ShouldEqual("macro2");
            macro.Value.Content.ShouldEqual("content 2");
            macro.Value.Description.ShouldEqual("description 2");
        };

        It should_clone_2nd_macro_with_different_reference = () =>
            clonedDocument.Macros.ElementAt(1).ShouldNotBeTheSameAs(document.Macros.ElementAt(1));

        private static QuestionnaireDocument document;
        private static QuestionnaireDocument clonedDocument;
    }
}
