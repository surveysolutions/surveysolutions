using System;
using Main.Core.Documents;
using Main.Core.View;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class JsonExportService : IJsonExportService
    {
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;
        private readonly IQuestionnaireVersioner versioner;

        public JsonExportService(IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory,
            IQuestionnaireVersioner versioner)
        {
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.versioner = versioner;
        }

        public TemplateInfo GetQuestionnaireTemplateInfo(Guid templateId)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(templateId));
            if (questionnaireView == null)
                return null;

            return this.GetQuestionnaireTemplateInfo(questionnaireView.Source);
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
