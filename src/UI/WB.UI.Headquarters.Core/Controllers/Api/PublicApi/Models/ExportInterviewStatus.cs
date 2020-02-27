using System.Text.Json.Serialization;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ExportInterviewStatus
    {
        SupervisorAssigned = 40,
        InterviewerAssigned = 60,
        Completed = 100,

        RejectedBySupervisor = 65,
        ApprovedBySupervisor = 120,

        RejectedByHeadquarters = 125,
        ApprovedByHeadquarters = 130
    }
}
