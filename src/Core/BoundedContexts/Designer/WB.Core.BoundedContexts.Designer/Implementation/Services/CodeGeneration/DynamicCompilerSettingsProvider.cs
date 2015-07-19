using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class DynamicCompilerSettingsProvider : IDynamicCompilerSettingsProvider
    {
        readonly IDynamicCompilerSettingsGroup settingsGroup;
        readonly IFileSystemAccessor fileSystemAccessor;

        public DynamicCompilerSettingsProvider(IDynamicCompilerSettingsGroup settingsGroup, IFileSystemAccessor fileSystemAccessor)
        {
            this.settingsGroup = settingsGroup;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public IEnumerable<PortableExecutableReference> GetAssembliesToRoslyn(Version apiVersion)
        {
            if (apiVersion.Major < 8)
                return this.ReturnReferencesForProfile24();

            return ReturnReferencesForProfile111();
        }

        IEnumerable<PortableExecutableReference> ReturnReferencesForProfile111()
        {
            var references = new List<PortableExecutableReference>();
            references.Add(AssemblyMetadata.CreateFromFile(typeof(Identity).Assembly.Location).GetReference());
            references.AddRange(this.GetPathToAssemblies("profile111"));
            return references;
        }

        IEnumerable<PortableExecutableReference> ReturnReferencesForProfile24()
        {
            var references = new List<PortableExecutableReference>();
            references.Add(AssemblyMetadata.CreateFromImage(
                    RoslynCompilerResources.WB_Core_SharedKernels_DataCollection_Portable_Profile24
                    ).GetReference());
            references.AddRange(this.GetPathToAssemblies("profile24"));
            return references;
        }

        IEnumerable<PortableExecutableReference> GetPathToAssemblies(string profileName)
        {
            var list = settingsGroup.SettingsCollection.ToList();

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