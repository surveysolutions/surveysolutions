using System;
using System.Collections.Generic;
using Main.Core.Commands.Questionnaire.Completed;
using WB.UI.Shared.Web.CommandDeserialization;

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
                        {"CreateInterviewWithFeaturedQuestionsCommand", typeof (CreateInterviewWithFeaturedQuestionsCommand)}
                    };
            }
        }
    }
}
