using System;

namespace Questionnaire.Core.Web.Export
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IDataExport
    {
        byte[] ExportData(Guid templateGuid, string type);
    }
}