using System;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IQuestionnareAssemblyFileAccessor
    {
        string GetFullPathToAssembly(Guid questionnaireId, long questionnaireVersion);

        void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64);

        void RemoveAssembly(Guid questionnaireId, long questionnaireVersion);

        string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion);

        byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion);
    }
}