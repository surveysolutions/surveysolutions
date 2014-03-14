using WB.Core.BoundedContexts.Headquarters.Events.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Events.Survey
{
    public class SupervisorRegistered : SurveyEvent
    {
        public SupervisorRegistered(string login, string password)
        {
            this.Login = login;
            this.Password = password;
        }

        public string Login { get; set; }
        public string Password{ get; set; }
    }
}
