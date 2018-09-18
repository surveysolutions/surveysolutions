namespace WB.Services.Export.CsvExport.Exporters
{
    public enum InterviewExportedAction
    {
        SupervisorAssigned = 0,
        InterviewerAssigned = 1,
        FirstAnswerSet = 2,
        Completed = 3,
        Restarted = 4,
        ApprovedBySupervisor = 5,
        ApprovedByHeadquarter = 6,
        RejectedBySupervisor = 7,
        RejectedByHeadquarter = 8,
        Deleted = 9,
        Restored = 10,
        UnapprovedByHeadquarter = 11,
        Created = 12,
        InterviewReceivedByTablet = 13,
        Resumed = 14,
        Paused = 15,
        TranslationSwitched = 16,
        OpenedBySupervisor = 17,
        ClosedBySupervisor = 18
    }
}