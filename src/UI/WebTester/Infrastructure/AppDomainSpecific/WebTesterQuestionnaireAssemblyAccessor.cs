using System;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        public string AssmeblyAsBase64StringProp;
        private Assembly assembly;

        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            if (assembly == null)
            {
                assembly = AppDomain.CurrentDomain.Load(Convert.FromBase64String(AssmeblyAsBase64StringProp));
            }

            return assembly;
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            AssmeblyAsBase64StringProp = assemblyAsBase64;
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
            AssmeblyAsBase64StringProp = null;
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            AssmeblyAsBase64StringProp = null;
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            return AssmeblyAsBase64StringProp;
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            return Convert.FromBase64String(AssmeblyAsBase64StringProp);
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            return AssmeblyAsBase64StringProp != null;
        }
    }
}