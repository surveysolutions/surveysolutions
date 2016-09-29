using System;
using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Accessors
{
    public interface IQuestionnaireAssemblyFileAccessor
    {
        string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion);

        void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64);
        void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly);

        Task StoreAssemblyAsync(QuestionnaireIdentity  questionnaireIdentity, byte[] assembly);

        void RemoveAssembly(Guid questionnaireId, long questionnaireVersion);

        void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity);

        string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion);

        byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion);

        bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion);
    }
}