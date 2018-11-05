using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class QuestionnaireVersion : AppSetting
    {
        public int MinQuestionnaireVersionSupportedByInterviewer { get; set; }
    }
}