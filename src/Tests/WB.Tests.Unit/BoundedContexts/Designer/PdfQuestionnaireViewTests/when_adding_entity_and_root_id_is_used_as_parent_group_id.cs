using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireViewTests
{
    internal class when_adding_entity_and_root_id_is_used_as_parent_group_id : PdfQuestionnaireViewTestsContext
    {
        Establish context = () =>
        {
            root = Create.PdfQuestionnaireView(rootId);
        };

        Because of = () =>
            root.AddEntity(addedEntity, rootId);

        It should_place_added_entity_directly_under_root_as_last_element = () =>
            root.Children.Last().ShouldEqual(addedEntity);

        private static PdfQuestionnaireView root;
        private static Guid? rootId = Guid.Parse("33333333333333333333333333333333");
        private static PdfEntityView addedEntity = Create.PdfQuestionView();
    }
}