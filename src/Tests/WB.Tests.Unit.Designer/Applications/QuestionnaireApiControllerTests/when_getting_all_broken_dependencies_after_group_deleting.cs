using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Api;
using WB.UI.Designer.Controllers.Api.Designer;


namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class when_getting_all_broken_dependencies_after_group_deleting : QuestionnaireApiControllerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            brokenLinks = new List<QuestionnaireItemLink>
            {
                CreateQuestionnaireItemLink(),
                CreateQuestionnaireItemLink(),
                CreateQuestionnaireItemLink()
            };
            questionnaireInfoFactory = Mock.Of<IQuestionnaireInfoFactory>(
                x => x.GetAllBrokenGroupDependencies(questionnaireId, groupId) == brokenLinks);

            controller = CreateQuestionnaireController(questionnaireInfoFactory: questionnaireInfoFactory);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = controller.GetAllBrokenGroupDependencies(questionnaireId, groupId);

        [NUnit.Framework.Test] public void should_return_not_null_result () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_list_of_links_returned_by_factory () =>
            result.Should().BeEquivalentTo(brokenLinks);

        private static QuestionnaireApiController controller;
        private static List<QuestionnaireItemLink> result;
        private static QuestionnaireRevision questionnaireId = Create.QuestionnaireRevision("22222222222222222222222222222222");
        private static Guid groupId = Guid.Parse("33333333333333333333333333333333");
        private static IQuestionnaireInfoFactory questionnaireInfoFactory;
        private static List<QuestionnaireItemLink> brokenLinks;
    }
}
