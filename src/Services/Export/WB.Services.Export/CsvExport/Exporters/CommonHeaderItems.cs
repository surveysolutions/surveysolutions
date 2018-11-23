using WB.Services.Export.Interview;

namespace WB.Services.Export.CsvExport.Exporters
{
    public static class CommonHeaderItems
    {
        public static readonly DoExportFileHeader InterviewId =
            new DoExportFileHeader("interview__id", "Unique 32-character long identifier of the interview", ExportValueType.String);

        public static readonly DoExportFileHeader InterviewKey =
            new DoExportFileHeader("interview__key", "Interview key (identifier in XX-XX-XX-XX format)", ExportValueType.String);

        public static readonly DoExportFileHeader Roster =
            new DoExportFileHeader("roster", "Name of the roster containing the variable", ExportValueType.String);

        public static readonly DoExportFileHeader Id1 =
            new DoExportFileHeader("id1", "Roster ID of the 1st level of nesting", ExportValueType.String, true);

        public static readonly DoExportFileHeader Id2 =
            new DoExportFileHeader("id2", "Roster ID of the 2nd level of nesting", ExportValueType.String, true);

        public static readonly DoExportFileHeader Id3 =
            new DoExportFileHeader("id3", "Roster ID of the 3rd level of nesting", ExportValueType.String, true);

        public static readonly DoExportFileHeader Id4 =
            new DoExportFileHeader("id4", "Roster ID of the 4th level of nesting", ExportValueType.String, true);
    }
}
