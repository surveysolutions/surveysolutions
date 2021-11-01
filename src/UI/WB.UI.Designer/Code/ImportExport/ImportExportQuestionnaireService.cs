using System;
using System.Collections.Generic;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
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

                FixAfterMapping(questionnaireDocument);

                return questionnaireDocument;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static void FixAfterMapping(QuestionnaireDocument questionnaireDocument)
        {
            questionnaireDocument.ForEachTreeElement<IComposite>(c => c.Children, (parent, child) =>
            {
                if (child is Main.Core.Entities.SubEntities.IQuestion question)
                {
                    if (question.LinkedToQuestionId.HasValue &&
                        question.LinkedToQuestionId == question.LinkedToRosterId)
                    {
                        var linkedToQuestion = questionnaireDocument.Find<Main.Core.Entities.SubEntities.IQuestion>(
                            question.LinkedToQuestionId.Value);
                        question.LinkedToQuestionId = linkedToQuestion?.PublicKey;
                        var linkedToRoster = questionnaireDocument.Find<Main.Core.Entities.SubEntities.IGroup>(
                            question.LinkedToRosterId.Value);
                        question.LinkedToRosterId = linkedToRoster?.PublicKey;
                    }
                }
            });
        }

        public QuestionnaireDocument Append(string json, QuestionnaireDocument destination)
        {
            try
            {
                var questionnaire = JsonConvert.DeserializeObject<Questionnaire>(json);
                var questionnaireDocument = mapper.Map(questionnaire, destination);

                FixAfterMapping(questionnaireDocument);

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