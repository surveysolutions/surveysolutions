using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Designer.Api;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireApiControllerTests
{
    internal class when_getting_all_broken_dependencies_after_group_deleting : QuestionnaireApiControllerTestContext
    {
        Establish context = () =>
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
        };

        Because of = () =>
            result = controller.GetAllBrokenGroupDependencies(questionnaireId, groupId);

        It should_return_not_null_result = () =>
            result.ShouldNotBeNull();

        It should_return_list_of_links_returned_by_factory = () =>
            result.ShouldEqual(brokenLinks);

        private static QuestionnaireController controller;
        private static List<QuestionnaireItemLink> result;
        private static string questionnaireId = Guid.Parse("22222222222222222222222222222222").FormatGuid();
        private static Guid groupId = Guid.Parse("33333333333333333333333333333333");
        private static IQuestionnaireInfoFactory questionnaireInfoFactory;
        private static List<QuestionnaireItemLink> brokenLinks;
    }
}