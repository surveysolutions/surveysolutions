using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.FileSystem;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DefaultDynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        readonly IFileSystemAccessor fileSystemAccessor;

        public DefaultDynamicCompilerSettingsProvider(IFileSystemAccessor fileSystemAccessor)
        {
            this.fileSystemAccessor = fileSystemAccessor;
        }

        IDynamicCompilerSettings dynamicCompilerSettings = new DefaultDynamicCompilerSettings();

        public IDynamicCompilerSettings DynamicCompilerSettings
        {
            get { return this.dynamicCompilerSettings; }
            set { this.dynamicCompilerSettings = value; }
        }

        public IEnumerable<PortableExecutableReference> GetAssembliesToRoslyn(Version apiVersion)
        {
            return GetMetadataReferences(DynamicCompilerSettings);
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
