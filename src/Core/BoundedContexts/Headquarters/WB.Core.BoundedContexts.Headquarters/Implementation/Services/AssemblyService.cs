using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    public class AssemblyService : IAssemblyService
    {
        private readonly IPlainStorageAccessor<AssemblyInfo> assemblyInfosStorage;

        public AssemblyService(IPlainStorageAccessor<AssemblyInfo> assemblyInfosStorage)
        {
            this.assemblyInfosStorage = assemblyInfosStorage;
        }

        public void SaveAssemblyInfo(string assemblyId, DateTime created, byte[] content)
            => this.assemblyInfosStorage.Store(new AssemblyInfo
            {
                AssemblyId = assemblyId,
                CreationDate = created,
                Content = content
            }, assemblyId);

        public void DeleteAssemblyInfo(string assemblyId) => this.assemblyInfosStorage.Remove(assemblyId);
        
        public AssemblyInfo GetAssemblyInfo(string assemblyId) => this.assemblyInfosStorage.GetById(assemblyId);
    }
}
