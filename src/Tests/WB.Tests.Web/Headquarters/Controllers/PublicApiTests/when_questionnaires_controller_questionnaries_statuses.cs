using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using WB.UI.Headquarters.API.PublicApi;


namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    internal class when_questionnaires_controller_questionnaries_statuses : ApiTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            controller = CreateQuestionnairesController();
            BecauseOf();
        }
        
        public void BecauseOf() 
        {
            actionResult = controller.QuestionnairesStatuses();
        }

        [NUnit.Framework.Test] public void should_return_IEnumerable_string () =>
            actionResult.Should().BeAssignableTo<IEnumerable<string>>();

        [NUnit.Framework.Test] public void should_count_be_positive () =>
            actionResult.Count().Should().BeGreaterThan(0);
        
        private static IEnumerable<string> actionResult;
        private static QuestionnairesController controller;
    }
}
