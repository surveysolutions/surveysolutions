using System.Reflection;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IMigrationRunner
    {
        void MigrateUp(params Assembly[] scanInAssembly);
    }
}
