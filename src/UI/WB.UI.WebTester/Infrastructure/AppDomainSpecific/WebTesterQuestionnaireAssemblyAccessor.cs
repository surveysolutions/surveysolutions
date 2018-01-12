using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Infrastructure
{
    public class WebTesterQuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        public string assmeblyAsBase64StringProp;

        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            var bytes = Convert.FromBase64String(assmeblyAsBase64StringProp);

            return Assembly.Load(bytes);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            assmeblyAsBase64StringProp = assemblyAsBase64;
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
            assmeblyAsBase64StringProp = null;
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            assmeblyAsBase64StringProp = null;
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            return assmeblyAsBase64StringProp;
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            return Convert.FromBase64String(assmeblyAsBase64StringProp);
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            return assmeblyAsBase64StringProp != null;
        }
    }
}