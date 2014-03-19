using WB.Core.BoundedContexts.Headquarters.Events.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Events.Survey
{
    public class SupervisorRegistered : SurveyEvent
    {
        public SupervisorRegistered(string login, string passwordHash)
        {
            this.Login = login;
            this.PasswordHash = passwordHash;
        }

        public string Login { get; set; }
        public string PasswordHash{ get; set; }
    }
}
