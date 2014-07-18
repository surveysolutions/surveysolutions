using System.CodeDom.Compiler;

namespace WB.Core.Infrastructure.Compilation
{
    interface ICodeGenerator
    {
        string Generate();
    }
}
