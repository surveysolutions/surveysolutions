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

        public static string GetDDIFileName(string name)
        {
            return string.Format("{0}{1}", name, ExtensionOfDDIMetaDataFile);
        }

        public static string ExtensionOfExportedDataFile { get { return ".tab"; } }
        public static string ExtensionOfDDIMetaDataFile { get { return ".xml"; } }

        public static char SeparatorOfExportedDataFile { get { return '\t'; } }
    }
}
