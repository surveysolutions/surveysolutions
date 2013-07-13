using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Commands.Questionnaire;
using Main.Core.Commands.Questionnaire.Group;
using Main.Core.Commands.Questionnaire.Question;

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
                        {"UpdateQuestionnaire", typeof (UpdateQuestionnaireCommand)}
                    };
            }
        }
    }
}
