using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        readonly ICompilerSettings settings;
        readonly IFileSystemAccessor fileSystemAccessor;

        public DynamicCompilerSettingsProvider(ICompilerSettings settings, IFileSystemAccessor fileSystemAccessor)
        {
            this.settings = settings;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public List<PortableExecutableReference> GetAssembliesToReference()
        {
            var references = new List<PortableExecutableReference>();
            references.Add(AssemblyMetadata.CreateFromFile(typeof(Identity).Assembly.Location).GetReference());
            references.AddRange(this.GetPathToAssemblies("profile111"));
            return references;
        }

        IEnumerable<PortableExecutableReference> GetPathToAssemblies(string profileName)
        {
            var list = this.settings.SettingsCollection.ToList();

            IDynamicCompilerSettings setting = list.Single(i => i.Name == profileName);

            return GetMetadataReferences(setting);
        }

        IEnumerable<PortableExecutableReference> GetMetadataReferences(IDynamicCompilerSettings settings)
        {
            return settings.DefaultReferencedPortableAssemblies.Select(
                defaultReferencedPortableAssembly =>
                    AssemblyMetadata.CreateFromFile(
                        this.fileSystemAccessor.CombinePath(
                            settings.PortableAssembliesPath,
                            defaultReferencedPortableAssembly
                        )
                    ).GetReference()
            );
        }
    }
}