using System;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Newtonsoft.Json;
using Formatting = System.Xml.Formatting;

namespace WB.Core.Questionnaire.ExportServices
{
    public class JsonExportService : IExportService
    {
        private readonly IDenormalizerStorage<QuestionnaireDocument> questionnaireStorage;

        public JsonExportService(IDenormalizerStorage<QuestionnaireDocument> questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public string GetQuestionnaireTemplate(Guid templateId)
        {
            var template = questionnaireStorage.GetByGuid(templateId);
            if (template == null)
                return null;
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            return JsonConvert.SerializeObject(template, Newtonsoft.Json.Formatting.Indented, settings);
          
        }
    }
}
