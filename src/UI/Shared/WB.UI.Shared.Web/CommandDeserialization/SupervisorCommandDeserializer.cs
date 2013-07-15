using System;
using System.Collections.Generic;
using Main.Core.Commands.Questionnaire.Completed;

namespace WB.UI.Shared.Web.CommandDeserialization
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
