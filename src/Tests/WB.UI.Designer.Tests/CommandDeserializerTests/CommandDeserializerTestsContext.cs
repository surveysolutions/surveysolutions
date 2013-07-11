using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.UI.Designer.Tests.CommandDeserializerTests
{
    using Machine.Specifications;
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
