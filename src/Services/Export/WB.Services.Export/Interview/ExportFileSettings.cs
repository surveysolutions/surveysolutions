namespace WB.Services.Export.Interview
{
    public class ExportFileSettings
    {
        public static string GetContentFileName(string levelName) => $"{levelName}{DataFileExtension}";

        public static string GetDDIFileName(string name) => $"{name}{DDIMetaDataFileExtension}";

        public const string DataFileExtension = TabDataFileExtension;
        public const string TabDataFileExtension = ".tab";
        public const string SpssDataFileExtension = ".sav";
        public const string StataDataFileExtension = ".dta";
        public const string DDIMetaDataFileExtension = ".xml";
        public const char DataFileSeparator = '\t';
        public const char NotReadableAnswersSeparator = '\u263A';
    }
}