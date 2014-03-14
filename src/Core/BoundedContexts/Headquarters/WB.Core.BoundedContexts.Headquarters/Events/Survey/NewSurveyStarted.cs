using WB.Core.BoundedContexts.Headquarters.Events.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Events.Survey
{
    public class NewSurveyStarted : SurveyEvent
    {
        public string Name { get; private set; }

        public NewSurveyStarted(string name)
        {
            this.Name = name;
        }
    }
}