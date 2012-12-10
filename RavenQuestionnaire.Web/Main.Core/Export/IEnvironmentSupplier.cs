// -----------------------------------------------------------------------
// <copyright file="IVarListBuilder.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Main.Core.Export
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IEnvironmentSupplier<T>
    {
        string BuildContent(T result, string parentPrimaryKeyName, string fileName, FileType type);
        void AddCompledResults(IDictionary<string,byte[]> container);
        
    }

    public enum FileType
    {
        Tab,
        Csv
    }
}
