using System;
using System.Collections.Generic;
using AutoMapper;
using Main.Core.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Designer.Code.ImportExport.Models;

namespace WB.UI.Designer.Code.ImportExport
{
    public class ImportExportQuestionnaireService : IImportExportQuestionnaireService
    {
        //private readonly IQuestionnaireViewFactory questionnaireStorage;
        private readonly IMapper mapper;
        private readonly ISerializer serializer;

        public ImportExportQuestionnaireService(//IQuestionnaireViewFactory questionnaireStorage,
            IMapper mapper,
            ISerializer serializer)
        {
            //this.questionnaireStorage = questionnaireStorage;
            this.mapper = mapper;
            this.serializer = serializer;
        }

        /*public string Export(Guid questionnaireId)
        {
            var questionnaireView = questionnaireStorage.Load(new QuestionnaireViewInputModel(questionnaireId));
            if (questionnaireView == null)
                throw new ArgumentException();
            
            var questionnaireDocument = questionnaireView.Source;

            try
            {
                var map = mapper.Map<Models.Questionnaire>(questionnaireDocument);

                var json = serializer.Serialize(map);
                return json;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }*/
        
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
                }

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
                var questionnaire = serializer.Deserialize<Questionnaire>(json);
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