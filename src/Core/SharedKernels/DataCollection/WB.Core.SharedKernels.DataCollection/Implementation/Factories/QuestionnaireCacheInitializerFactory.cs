using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Factories
{
    internal class QuestionnaireCacheInitializerFactory : IQuestionnaireCacheInitializerFactory
    {
        private readonly IExpressionProcessor expressionProcessor;

        public QuestionnaireCacheInitializerFactory(IExpressionProcessor expressionProcessor)
        {
            this.expressionProcessor = expressionProcessor;
        }

        public IQuestionnaireCacheInitializer CreateQuestionnaireCacheInitializer(QuestionnaireDocument document)
        {
            return new QuestionnaireCacheInitializer(document, expressionProcessor);
        }
    }
}
