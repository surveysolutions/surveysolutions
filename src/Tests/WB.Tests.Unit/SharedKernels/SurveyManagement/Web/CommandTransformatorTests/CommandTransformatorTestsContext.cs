using WB.Core.SharedKernels.SurveyManagement.Web.Code.CommandTransformation;

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
