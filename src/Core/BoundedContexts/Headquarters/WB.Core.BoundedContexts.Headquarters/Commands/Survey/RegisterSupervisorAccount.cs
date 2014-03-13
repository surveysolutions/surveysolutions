using System;
using WB.Core.BoundedContexts.Headquarters.Commands.Survey.Base;

namespace WB.Core.BoundedContexts.Headquarters.Commands.Survey
{
    public class RegisterSupervisorAccount : SurveyCommand
    {
        public RegisterSupervisorAccount(Guid surveyId) : base(surveyId)
        {
        }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ApplicationUrl { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}