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
            return $"{levelName}{ExtensionOfExportedDataFile}";
        }

        public static string GetDDIFileName(string name)
        {
            return $"{name}{ExtensionOfDDIMetaDataFile}";
        }

        public static string ExtensionOfExportedDataFile => ".tab";
        public static string ExtensionOfDDIMetaDataFile => ".xml";
        public static char SeparatorOfExportedDataFile => '\t';
        public static char NotReadableAnswersSeparator => '\u263A';
    }
}
