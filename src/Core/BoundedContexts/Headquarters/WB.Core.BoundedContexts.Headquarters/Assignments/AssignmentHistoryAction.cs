using System.Text.Json.Serialization;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    // must be equal to enum from ExportService
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AssignmentHistoryAction
    {
        Unknown = 0,
        Created = 1,
        Archived = 2,
        Deleted = 3,
        ReceivedByTablet = 4,
        UnArchived = 5,
        AudioRecordingChanged = 6,
        Reassigned = 7,
        QuantityChanged = 8,
        WebModeChanged = 9
    }
}
