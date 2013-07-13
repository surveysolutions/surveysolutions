using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.Tests.CommandDeserializerTests
{
    using Machine.Specifications;

    using Main.Core.Commands.Questionnaire.Group;

    using Ncqrs.Commanding;

    using WB.UI.Designer.Code.Helpers;

    [Subject(typeof(CommandDeserializer))]
    internal class CommandDeserializerTestsContext
    {
        public static CommandDeserializer CreateCommandDeserializer()
        {
            return new CommandDeserializer();
        }
    }
}
