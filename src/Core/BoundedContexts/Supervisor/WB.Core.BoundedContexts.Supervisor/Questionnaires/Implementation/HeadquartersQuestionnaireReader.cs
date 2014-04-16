using System;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;

namespace WB.Core.BoundedContexts.Supervisor.Questionnaires.Implementation
{
    internal class HeadquartersQuestionnaireReader : HeadquartersEntityReader, IHeadquartersQuestionnaireReader
    {
        public async Task<QuestionnaireDocument> GetQuestionnaireByUri(Uri headquartersQuestionnaireUri)
        {
            return await GetEntityByUri<QuestionnaireDocument>(headquartersQuestionnaireUri);
        }
    }
}