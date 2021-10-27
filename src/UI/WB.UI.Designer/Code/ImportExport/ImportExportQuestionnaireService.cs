using System;
using System.Collections.Generic;
using AutoMapper;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Code.ImportExport.Models;

namespace WB.UI.Designer.Code.ImportExport
{
    public class ImportExportQuestionnaireService : IImportExportQuestionnaireService
    {
        //private readonly IQuestionnaireViewFactory questionnaireStorage;
        private readonly IMapper mapper;
        private readonly ISerializer serializer;

        public ImportExportQuestionnaireService(
            IMapper mapper,
            ISerializer serializer)
        {
            this.mapper = mapper;
            this.serializer = serializer;
        }

        private static readonly JsonSerializerSettings jsonSerializerSettings = 
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                },
            };
        
        public string Export(QuestionnaireDocument questionnaireDocument)
        {
            try
            {
                var map = mapper.Map<Models.Questionnaire>(questionnaireDocument);
                var json = JsonConvert.SerializeObject(map, jsonSerializerSettings);
                //var json = serializer.Serialize(map);
                return json;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public QuestionnaireDocument Import(string json)
        {
            try
            {
                //var questionnaire = serializer.Deserialize<Questionnaire>(json);
                var questionnaire = JsonConvert.DeserializeObject<Questionnaire>(json);
                var questionnaireDocument = mapper.Map<QuestionnaireDocument>(questionnaire);

                return questionnaireDocument;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}