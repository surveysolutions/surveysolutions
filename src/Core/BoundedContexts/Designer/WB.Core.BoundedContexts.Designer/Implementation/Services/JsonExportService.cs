using System;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class JsonExportService : IJsonExportService
    {
        #warning ViewFactory should be used here
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage;
        private readonly IQuestionnaireVersioner versioner;

        public JsonExportService(IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage,
            IQuestionnaireVersioner versioner)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.versioner = versioner;
        }

        public TemplateInfo GetQuestionnaireTemplate(Guid templateId)
        {
            var template = questionnaireStorage.GetById(templateId);
            return this.GetQuestionnaireTemplate(template);
        }

        public TemplateInfo GetQuestionnaireTemplate(QuestionnaireDocument template)
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
