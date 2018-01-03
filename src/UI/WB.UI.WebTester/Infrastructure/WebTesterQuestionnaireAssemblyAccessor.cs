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
        private static readonly Dictionary<string, string> Assemblies = new Dictionary<string, string>();

        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            var identity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            var assembly = Assemblies[identity.ToString()];
            var bytes = Convert.FromBase64String(assembly);

            return Assembly.Load(bytes);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            var identity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            Assemblies[identity.ToString()] = assemblyAsBase64;
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
            var identity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            Assemblies.Remove(identity.ToString());
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            Assemblies.Remove(questionnaireIdentity.ToString());
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            var identity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            return Assemblies[identity.ToString()];
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            var identity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            return Convert.FromBase64String(Assemblies[identity.ToString()]);
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            var identity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            return Assemblies.ContainsKey(identity.ToString());
        }
    }
}