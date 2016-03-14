using WB.UI.Designer.CommandDeserialization;

namespace WB.Tests.Unit.Designer.Applications.CommandDeserializerTests
{
    internal class CommandDeserializerTestsContext
    {
        public static DesignerCommandDeserializer CreateCommandDeserializer()
        {
            return new DesignerCommandDeserializer();
        }
    }
}
