using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireExportService  : IQuestionnaireExportService
    {
        private readonly IQuestionnaireVersioner versioner;

        public QuestionnaireExportService(IQuestionnaireVersioner versioner)
        {
            this.versioner = versioner;
        }

        public TemplateInfo GetQuestionnaireTemplateInfo(QuestionnaireDocument template)
        {
            if (template == null) 
                return null;

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            return new TemplateInfo()
            {
                Title = template.Title,
                Source = JsonConvert.SerializeObject(template, Formatting.Indented, settings),
                Version = versioner.GetVersion(template)
            };

        }
    }
}
