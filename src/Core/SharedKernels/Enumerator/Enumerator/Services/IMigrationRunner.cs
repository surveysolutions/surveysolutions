using System.Reflection;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IMigrationRunner
    {
        void MigrateUp(string appName, params Assembly[] scanInAssembly);
    }
}
