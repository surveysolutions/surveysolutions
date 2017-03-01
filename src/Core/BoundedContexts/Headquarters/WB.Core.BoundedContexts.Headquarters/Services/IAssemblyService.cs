using System;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IAssemblyService
    {
        void SaveAssemblyInfo(string assemblyId, DateTime created, byte[] content);

        AssemblyInfo GetAssemblyInfo(string assemblyId);

        void DeleteAssemblyInfo(string contentHash);
    }
}