using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Commands.Survey
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Survey), "RegisterSupervisorAccount")]
    public class RegisterSupervisorAccount : SurveyCommand
    {
        public RegisterSupervisorAccount(Guid surveyId, string login, string password) : base(surveyId)
        {
            this.Login = login;
            this.Password = password;
        }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}