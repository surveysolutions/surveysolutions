using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public class TemplateInfo
    {
        public string Title { get; set; }

        public string Source { get; set; }
        
        public QuestionnaireVersion Version { get; set; }
    }
}