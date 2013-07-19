using System;
using System.Collections.Generic;
using Main.Core.Commands.Questionnaire.Completed;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.UI.Shared.Web.CommandDeserialization;
using Web.Supervisor.Models;

namespace Web.Supervisor.CommandDeserialization
{
    internal class SupervisorCommandDeserializer : CommandDeserializer
    {
        protected override Dictionary<string, Type> KnownCommandTypes
        {
            get
            {
                return new Dictionary<string, Type>
                    {
                        { "CreateInterviewWithFeaturedQuestionsCommand", typeof (CreateInterviewWithFeaturedQuestionsCommand) },
                        { "AssignInterviewToUserCommand", typeof (AssignInterviewToUserCommand) }
                    };
            }
        }
    }
}
