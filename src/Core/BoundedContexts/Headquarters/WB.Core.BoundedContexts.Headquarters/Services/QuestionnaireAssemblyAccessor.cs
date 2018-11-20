using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    internal class QuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        private readonly IAssemblyService assemblyService;
        private readonly ILogger logger;
        private static readonly ConcurrentDictionary<string, AssemblyHolder> assemblyCache = new ConcurrentDictionary<string, AssemblyHolder>();

        public QuestionnaireAssemblyAccessor(IAssemblyService assemblyService, ILogger logger)
        {
            this.assemblyService = assemblyService;
            this.logger = logger;
        }


        private class AssemblyHolder
        {
            public AssemblyHolder(string assemblyFileName, byte[] assemblyContent)
            {
                this.FileName = assemblyFileName;
                
                this.AssemblyContent = assemblyContent;
            }

            private Assembly assembly = null;

            public string FileName { private set; get; }

            public Assembly Assembly
            {
                get
                {
                    if(assembly == null)
                        assembly = Assembly.Load(AssemblyContent);
                    return this.assembly;

                } 
            }
            public byte[] AssemblyContent { private set; get; }
        }


        private AssemblyHolder GetAssemblyHolder(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var assembly = assemblyCache.GetOrAdd(assemblyFileName, CreateAssemblyHolder(assemblyFileName));

            return assembly;
        }

        public Assembly LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            var assembly = GetAssemblyHolder(questionnaireId, questionnaireVersion);
            return assembly?.Assembly;
        }

        private AssemblyHolder CreateAssemblyHolder(string assemblyFileName)
        {
            var assemblyInfo = this.assemblyService.GetAssemblyInfo(assemblyFileName);

            return assemblyInfo == null ? null : new AssemblyHolder(assemblyFileName, assemblyInfo.Content);
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, string assemblyAsBase64)
        {
            this.StoreAssembly(questionnaireId, questionnaireVersion, Convert.FromBase64String(assemblyAsBase64));
        }

        public void StoreAssembly(Guid questionnaireId, long questionnaireVersion, byte[] assembly)
        {
            if (assembly.Length == 0)
            {
                throw new ArgumentException($"Assembly file is empty. Cannot be saved. Questionnaire: {new QuestionnaireIdentity(questionnaireId, questionnaireVersion)}", 
                    nameof(assembly));
            }

            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);

            if (this.assemblyService.GetAssemblyInfo(assemblyFileName) != null)
            {
                throw new QuestionnaireAssemblyAlreadyExistsException(
                    "Questionnaire assembly file already exists and can not be overwritten",
                    new QuestionnaireIdentity(questionnaireId, questionnaireVersion));
            }

            this.assemblyService.SaveAssemblyInfo(assemblyFileName, DateTime.Now, assembly);
            
        }

        public Task StoreAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly)
        {
            throw new NotImplementedException();
        }

        public void RemoveAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            
            logger.Info($"Trying to delete assembly for questionnaire {new QuestionnaireIdentity(questionnaireId, questionnaireVersion)}");
            
            try
            {
                this.assemblyService.DeleteAssemblyInfo(assemblyFileName);
            }
            catch (IOException e)
            {
                logger.Error($"Error on assembly deletion for questionnaire {new QuestionnaireIdentity(questionnaireId, questionnaireVersion)}");
                logger.Error(e.Message, e);
            }
        }

        public void RemoveAssembly(QuestionnaireIdentity questionnaireIdentity)
        {
            throw new NotImplementedException();
        }

        public string GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            var assembly = GetAssemblyHolder(questionnaireId, questionnaireVersion);

            if (assembly == null)
                return null;

            return Convert.ToBase64String(assembly.AssemblyContent);
        }

        public byte[] GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
        {
            var assembly = GetAssemblyHolder(questionnaireId, questionnaireVersion);
            return assembly?.AssemblyContent;
        }

        public bool IsQuestionnaireAssemblyExists(Guid questionnaireId, long questionnaireVersion)
        {
            return GetAssemblyHolder(questionnaireId, questionnaireVersion) != null;
        }

        private string GetAssemblyFileName(Guid questionnaireId, long questionnaireVersion)
        {
            return String.Format("assembly_{0}_v{1}.dll", questionnaireId, questionnaireVersion);
        }
    }
}
