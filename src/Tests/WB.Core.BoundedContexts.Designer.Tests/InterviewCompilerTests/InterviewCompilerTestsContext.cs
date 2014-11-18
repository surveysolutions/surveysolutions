using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;

namespace WB.Core.BoundedContexts.Designer.Tests.InterviewCompilerTests
{
    internal class InterviewCompilerTestsContext
    {
        public static RoslynCompiler CreateRoslynCompiler()
        {
            return new RoslynCompiler(
                new DefaultDynamicCompillerSettings()
                {
                    PortableAssembliesPath =
                        "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111",
                    DefaultReferencedPortableAssemblies = new[] { "System.dll", "System.Core.dll", "mscorlib.dll", "System.Runtime.dll", 
                                "System.Collections.dll", "System.Linq.dll" }
                }, new FileSystemIOAccessor());
        }
    }
}
