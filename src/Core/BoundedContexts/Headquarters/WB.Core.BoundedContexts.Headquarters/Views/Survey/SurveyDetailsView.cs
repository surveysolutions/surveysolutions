using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
{
    public class SurveyDetailsView : IView
    {
        public SurveyDetailsView()
        {
            this.Supervisors = new List<SupervisorView>();
        }

        public string SurveyId { get; set; }
        public string Name { get; set; }

        public List<SupervisorView> Supervisors { get; set; }
    }
}