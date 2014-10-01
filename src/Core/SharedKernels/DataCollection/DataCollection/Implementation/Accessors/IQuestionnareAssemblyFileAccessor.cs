using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Accessors
{
    public interface IQuestionnaireAssemblyFileAccessor
    {
        string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion);

        void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64);

        void RemoveAssembly(Guid questionnaireId, long questionnaireVersion);

        string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion);

        byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion);
    }
}