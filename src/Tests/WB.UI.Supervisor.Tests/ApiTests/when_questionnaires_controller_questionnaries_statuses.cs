using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Supervisor.Tests.ApiTests
{
    internal class when_questionnaires_controller_questionnaries_statuses : ApiTestContext
    {
        private Establish context = () =>
        {
            controller = CreateQuestionnairesController();
        };
        
        Because of = () =>
        {
            actionResult = controller.QuestionnairesStatuses();
        };

        It should_return_IEnumerable_string = () =>
            actionResult.ShouldBeAssignableTo<IEnumerable<string>>();

        It should_count_be_positive = () =>
            actionResult.Count().ShouldBeGreaterThan(0);
        
        private static IEnumerable<string> actionResult;
        private static QuestionnairesController controller;
    }
}
