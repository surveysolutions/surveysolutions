#nullable enable
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Services.Impl
{
    internal class QuestionnaireAssemblyAccessor : IQuestionnaireAssemblyAccessor
    {
        private readonly IAssemblyService assemblyService;
        private readonly ILogger logger;
        private readonly IMemoryCache cache;

        public QuestionnaireAssemblyAccessor(IAssemblyService assemblyService, IMemoryCache cache, ILogger logger)
        {
            this.assemblyService = assemblyService;
            this.logger = logger;
            this.cache = cache;
        }

        [DebuggerDisplay("{FileName} - {Assembly}")]
        private class AssemblyHolder
        {
            public AssemblyHolder(string assemblyFileName, byte[] assemblyContent)
            {
                this.FileName = assemblyFileName;
                this.AssemblyContent = assemblyContent;
                assembly = new Lazy<Assembly?>(() =>
                {
                    var ass = Assembly.Load(assemblyContent);
                    var ctx = AssemblyLoadContext.GetLoadContext(ass);
                    if (ctx == null)
                    {
                        return null;
                    }

                    ctx.Resolving += (context, name) =>
                    {
                        // redirect binding to 
                        return context.Assemblies.FirstOrDefault(a => a.FullName == name.FullName);
                    };

                    return ass;
                });
            }

            private readonly Lazy<Assembly?> assembly;

            private string FileName { get; }

            public Assembly? Assembly => assembly.Value;

            public byte[] AssemblyContent { get; }
        }

        private string GetAssemblyCacheKey(string assemblyFileName)=> "Assembly-file-" + assemblyFileName;
        
        private AssemblyHolder? GetAssemblyHolder(Guid questionnaireId, long questionnaireVersion)
        {
            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);
            var assembly = cache.GetOrCreate(GetAssemblyCacheKey(assemblyFileName), entry => CreateAssemblyHolder(assemblyFileName));

            return assembly;
        }

        public Assembly? LoadAssembly(Guid questionnaireId, long questionnaireVersion)
        {
            var assembly = GetAssemblyHolder(questionnaireId, questionnaireVersion);
            return assembly?.Assembly;
        }

        private AssemblyHolder? CreateAssemblyHolder(string assemblyFileName)
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
                throw new ArgumentException($@"Assembly file is empty. Cannot be saved. Questionnaire: {new QuestionnaireIdentity(questionnaireId, questionnaireVersion)}", 
                    nameof(assembly));
            }

            string assemblyFileName = this.GetAssemblyFileName(questionnaireId, questionnaireVersion);

            if (this.assemblyService.GetAssemblyInfo(assemblyFileName) != null)
            {
                throw new QuestionnaireAssemblyAlreadyExistsException(
                    "Questionnaire assembly file already exists and cannot be overwritten",
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

        public string? GetAssemblyAsBase64String(Guid questionnaireId, long questionnaireVersion)
        {
            var assembly = GetAssemblyHolder(questionnaireId, questionnaireVersion);

            if (assembly == null)
                return null;

            return Convert.ToBase64String(assembly.AssemblyContent);
        }

        public byte[]? GetAssemblyAsByteArray(Guid questionnaireId, long questionnaireVersion)
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
            return $"assembly_{questionnaireId}_v{questionnaireVersion}.dll";
        }
    }
}
