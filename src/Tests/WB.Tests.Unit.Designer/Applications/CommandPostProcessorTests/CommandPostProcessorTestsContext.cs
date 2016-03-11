using System;
using Main.Core.Documents;

namespace WB.Tests.Unit.Applications.CommandPostProcessorTests
{
    internal class CommandPostProcessorTestsContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(string title, Guid creator)
        {
            return new QuestionnaireDocument(){Title = title, CreatedBy = creator};
        }
    }
}