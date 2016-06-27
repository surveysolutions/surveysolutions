using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Web.CommandTransformatorTests
{
    internal class CommandTransformatorTestsContext
    {
        public static CommandTransformator CreateCommandTransformator()
        {
            return new CommandTransformator();
        }
    }
}
