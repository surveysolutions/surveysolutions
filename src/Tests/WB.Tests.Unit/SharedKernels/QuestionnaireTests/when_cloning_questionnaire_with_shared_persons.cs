using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;

namespace WB.Tests.Unit.SharedKernels.QuestionnaireTests
{
    internal class when_cloning_questionnaire_with_shared_persons
    {
        Establish context = () =>
        {
            document = Create.QuestionnaireDocumentWithOneChapter();
            document.SharedPersons = new List<Guid>{ Id.g1, Id.g2 };
        };

        Because of = () =>
            clonedDocument = document.Clone();
        
        It should_clone_same_ids = () =>
            clonedDocument.SharedPersons.ShouldContainOnly(Id.g1, Id.g2);

        It should_clone_shared_persons_with_different_reference = () =>
            clonedDocument.SharedPersons.ShouldNotBeTheSameAs(document.SharedPersons);

        private static QuestionnaireDocument document;
        private static QuestionnaireDocument clonedDocument;
    }
}