namespace WB.Services.Export.CsvExport.Exporters
{
    // must be equal to enum from HQ
    public enum AssignmentExportedAction
    {
        Unknown = 0,
        Created = 1,
        Archived = 2,
        Deleted = 3,
        ReceivedByTablet = 4,
        Unarchived = 5,
        AudioRecordingChanged = 6,
        Reassigned = 7,
        QuantityChanged = 8,
        WebModeChanged = 9,
    }
}
