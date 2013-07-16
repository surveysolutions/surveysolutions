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

        public JsonExportService(IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public string GetQuestionnaireTemplate(Guid templateId)
        {
            var template = questionnaireStorage.GetById(templateId);
            if (template == null)
                return null;
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            return JsonConvert.SerializeObject(template, Newtonsoft.Json.Formatting.Indented, settings);
          
        }
    }
}
