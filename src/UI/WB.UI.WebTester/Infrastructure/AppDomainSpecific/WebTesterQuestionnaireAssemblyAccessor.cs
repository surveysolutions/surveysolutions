using System;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.WebTester.Services.Implementation;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        public Assembly? Assembly { get; set; }


        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            if(Assembly == null)
                throw new InvalidOperationException("Assembly must not be null.");
            return Assembly;
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly)
        {
        }

        public Task StoreAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            return Task.CompletedTask;
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            Assembly = null;
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            Assembly = null;
        }

        public string? GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            return null;
        }

        public byte[]? GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            return null;
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            return Assembly != null;
        }
    }
}
