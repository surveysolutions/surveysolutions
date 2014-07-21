using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireViewTests
{
    internal class when_adding_group_and_root_id_is_used_as_parent_id : PdfQuestionnaireViewTestsContext
    {
        Establish context = () =>
        {
            root = Create.PdfQuestionnaireView(rootId);
        };

        Because of = () =>
            root.AddGroup(addedGroup, rootId);

        It should_place_added_group_directly_under_root_as_last_element = () =>
            root.Children.Last().ShouldEqual(addedGroup);

        private static PdfQuestionnaireView root;
        private static Guid? rootId = Guid.Parse("33333333333333333333333333333333");
        private static PdfGroupView addedGroup = Create.PdfGroupView();
    }
}