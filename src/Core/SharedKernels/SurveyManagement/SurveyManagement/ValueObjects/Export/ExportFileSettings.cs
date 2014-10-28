using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export
{
    public class ExportFileSettings
    {
        public static string GetContentFileName(string levelName)
        {
            return string.Format("{0}{1}", levelName, ExtensionOfExportedDataFile);
        }

        public static string ExtensionOfExportedDataFile { get { return ".tab"; } }
        public static string SeparatorOfExportedDataFile { get { return "\t "; } }
    }
}
