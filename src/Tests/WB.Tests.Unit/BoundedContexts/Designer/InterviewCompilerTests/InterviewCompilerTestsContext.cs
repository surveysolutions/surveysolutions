using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

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
    }
}
