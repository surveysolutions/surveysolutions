using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.UI.Designer.CommandDeserialization;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.Tests.CommandDeserializerTests
{
    using Machine.Specifications;
    using Ncqrs.Commanding;

    using WB.UI.Designer.Code.Helpers;

    [Subject(typeof(DesignerCommandDeserializer))]
    internal class CommandDeserializerTestsContext
    {
        public static DesignerCommandDeserializer CreateCommandDeserializer()
        {
            return new DesignerCommandDeserializer();
        }
    }
}
