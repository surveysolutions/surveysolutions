using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class InterviewCompilerTestsContext
    {
        public static RoslynCompiler CreateRoslynCompiler()
        {
            return new RoslynCompiler(new FileSystemIOAccessor());
        }

        public static IDynamicCompilerSettings CreateDynamicCompillerSettings()
        {
            return new DefaultDynamicCompilerSettings()
            {
                PortableAssembliesPath =
                    "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111",
                DefaultReferencedPortableAssemblies = new[]
                {
                    "System.dll", 
                    "System.Core.dll", 
                    "mscorlib.dll", 
                    "System.Runtime.dll",
                    "System.Collections.dll",
                    "System.Linq.dll"
                }
            };
        }


        public static PortableExecutableReference[] CreateReferencesForCompiler()
        {
            var settings = CreateDynamicCompillerSettings();
            var fileAccessor = new FileSystemIOAccessor();
            var settingsProvider = new DefaultDynamicCompilerSettingsProvider(fileAccessor)
            {
                DynamicCompilerSettings = settings
            };

            var references = new List<PortableExecutableReference>();
            references.Add(AssemblyMetadata.CreateFromFile(typeof(Identity).Assembly.Location).GetReference());
            references.AddRange(settingsProvider.GetAssembliesToRoslyn(new Version()));
            return references.ToArray();

        }
    }
}
