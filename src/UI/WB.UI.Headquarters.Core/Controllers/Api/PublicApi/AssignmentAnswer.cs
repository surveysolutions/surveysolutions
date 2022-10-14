#nullable enable
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi;

public class AssignmentAnswer
{
    public AssignmentAnswer(AssignmentIdentifyingDataItem source, Identity questionIdentity)
    {
        Source = source;
        QuestionIdentity = questionIdentity;
    }

    public AssignmentIdentifyingDataItem Source { get; }
    public Identity QuestionIdentity { get; }
    public string? Variable { get; set; }

    public QuestionType? QuestionType { get; set; }

    public bool IsUnknownQuestion { get; set; }
}
