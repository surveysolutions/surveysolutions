using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
{
    public class SurveyDetailsView : IView
    {
        public SurveyDetailsView()
        {
            SupervisorAccounts = new List<SupervisorAccountView>();
        }

        public string SurveyId { get; set; }
        public string Name { get; set; }

        public List<SupervisorAccountView> SupervisorAccounts { get; set; }
    }
}