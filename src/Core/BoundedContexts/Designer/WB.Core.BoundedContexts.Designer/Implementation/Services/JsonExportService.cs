using System;
using Main.Core.Documents;
using Main.Core.Domain;
using Ncqrs.Commanding.ServiceModel;
using Newtonsoft.Json;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

using Formatting = System.Xml.Formatting;

namespace WB.Core.Questionnaire.ExportServices
{
    internal class JsonExportService : IExportService
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
