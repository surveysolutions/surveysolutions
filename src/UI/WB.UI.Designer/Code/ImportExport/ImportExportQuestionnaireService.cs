using System;
using AutoMapper;

using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Designer.Code.ImportExport
{
    public class ImportExportQuestionnaireService : IImportExportQuestionnaireService
    {
        private readonly IQuestionnaireViewFactory questionnaireStorage;
        private readonly IMapper mapper;
        private readonly ISerializer serializer;

        public ImportExportQuestionnaireService(IQuestionnaireViewFactory questionnaireStorage,
            IMapper mapper,
            ISerializer serializer)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.mapper = mapper;
            this.serializer = serializer;
        }

        public string Export(Guid questionnaireId)
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
        }
    }
}