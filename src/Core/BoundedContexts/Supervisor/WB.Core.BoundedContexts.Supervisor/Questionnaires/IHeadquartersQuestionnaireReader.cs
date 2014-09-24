using System;
using System.Threading.Tasks;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Supervisor.Questionnaires
{
    public interface IHeadquartersQuestionnaireReader
    {
        Task<QuestionnaireDocument> GetQuestionnaireByUri(Uri headquartersQuestionnaireUri);
        Task<byte[]> GetAssemblyByUri(Uri headquartersQuestionnaireAssemblyUri);
    }
}