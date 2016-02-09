using System;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Factories
{
    internal class QuestionnaireFactory : IQuestionnaireFactory
    {
        private readonly IServiceLocator serviceLocator;

        public QuestionnaireFactory(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public IQuestionnaire CreateTemporaryInstance(QuestionnaireDocument document)
        {
            var temporaryInstance = this.serviceLocator.GetInstance<Aggregates.Questionnaire>();
            temporaryInstance.Apply(new TemplateImported { Source = document });
            return temporaryInstance.GetQuestionnaire();
        }
    }
}