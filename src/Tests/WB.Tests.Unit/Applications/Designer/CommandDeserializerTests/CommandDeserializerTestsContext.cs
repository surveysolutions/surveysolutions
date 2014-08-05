using Machine.Specifications;
using WB.UI.Designer.CommandDeserialization;

namespace WB.Tests.Unit.Applications.Designer.CommandDeserializerTests
{
    internal class CommandDeserializerTestsContext
    {
        public static DesignerCommandDeserializer CreateCommandDeserializer()
        {
            return new DesignerCommandDeserializer();
        }
    }
}
