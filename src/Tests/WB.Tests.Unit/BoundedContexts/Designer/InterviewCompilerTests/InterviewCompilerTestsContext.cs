﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

namespace WB.Tests.Unit.BoundedContexts.Designer.InterviewCompilerTests
{
    internal class InterviewCompilerTestsContext
    {
        public static RoslynCompiler CreateRoslynCompiler()
        {
            return new RoslynCompiler();
        }

        public static IDynamicCompilerSettings CreateDynamicCompilerSettings()
        {
            return new DefaultDynamicCompilerSettings
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
            var fileAccessor = new FileSystemIOAccessor();

            const string pathToProfile = "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111";
            var referencesToAdd = new[] { "System.dll", "System.Core.dll", "mscorlib.dll", "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll" };

            var settings = new List<IDynamicCompilerSettings>
            {
                Mock.Of<IDynamicCompilerSettings>(_ 
                    => _.PortableAssembliesPath == pathToProfile
                    && _.DefaultReferencedPortableAssemblies == referencesToAdd 
                    && _.Name == "profile111")
            };

            var defaultDynamicCompilerSettings = Mock.Of<IDynamicCompilerSettingsGroup>(_ => _.SettingsCollection == settings);

            var settingsProvider = new DynamicCompilerSettingsProvider(defaultDynamicCompilerSettings, fileAccessor);

            return settingsProvider.GetAssembliesToRoslyn(new Version(8, 0, 0)).ToArray();
        }
    }
}
