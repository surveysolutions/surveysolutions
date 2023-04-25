using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Extensions.Entities;

public class StatusItem
{
    public StatusItem(InterviewStatus? status, string title)
    {
        Status = status;
        Title = title;
    }

    public string Title {private set; get; }
    public InterviewStatus? Status { private set; get; }
}