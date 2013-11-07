using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport
{
    public interface IEnvironmentSupplier<T>
    {
        void AddCompletedResults(IDictionary<string, byte[]> container);

        string BuildContent(T result, string parentPrimaryKeyName, string fileName, FileType type);
    }
}