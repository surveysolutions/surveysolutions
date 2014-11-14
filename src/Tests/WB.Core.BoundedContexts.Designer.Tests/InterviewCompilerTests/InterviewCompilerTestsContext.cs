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
                        "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.0\\Profile\\Profile24",
                    DefaultReferencedPortableAssemblies = new[] {"System.dll", "System.Core.dll", "mscorlib.dll"}
                }, new FileSystemIOAccessor());
        }
    }
}
