using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireDocumentUpgrader : IQuestionnaireDocumentUpgrader
    {
        private readonly IQuestionnaireEntityFactory questionnaireEntityFactory;

        public QuestionnaireDocumentUpgrader(IQuestionnaireEntityFactory questionnaireEntityFactory)
        {
            this.questionnaireEntityFactory = questionnaireEntityFactory;
        }
    }
}
