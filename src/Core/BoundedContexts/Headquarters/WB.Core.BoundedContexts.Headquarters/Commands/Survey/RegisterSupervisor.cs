using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Commands.Survey
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Survey), "RegisterSupervisor")]
    public class RegisterSupervisor : SurveyCommand
    {
        public RegisterSupervisor(Guid surveyId, string login, string password) : base(surveyId)
        {
            this.Login = login;
            this.Password = password;
        }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}