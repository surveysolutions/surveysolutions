using System;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            throw new NotImplementedException();
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public Task StoreAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            throw new NotImplementedException();
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            throw new NotImplementedException();
        }
    }
}