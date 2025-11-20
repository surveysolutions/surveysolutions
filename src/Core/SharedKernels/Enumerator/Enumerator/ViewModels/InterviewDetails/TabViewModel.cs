using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

public class TabViewModel : BaseViewModel
{
    public bool IsEnabled => Items.Count > 0;
    public bool ShowMore => Items.Count < Total;
    public string MoreCount => string.Format(UIResources.Interview_Complete_MoreCountString, Total - Items.Count);
    public string Title { get; set; }
    public CompleteTabContent TabContent { get; set; }
    public ObservableRangeCollection<EntityWithErrorsViewModel> Items { get; set; } = new();
        
    public string Count => Items.Count > 0 ? $"{Total}" : "No";
        
    private int total;
    public int Total
    {
        get => total;
        set
        {
            if (total != value)
            {
                total = value;
                RaisePropertyChanged(() => Total);
                RaisePropertyChanged(() => ShowMore);
                RaisePropertyChanged(() => MoreCount);
                RaisePropertyChanged(() => Count);
            }
        }
    }

    public override void Dispose()
    {
        Items.ForEach(i => i.DisposeIfDisposable());
        Items.DisposeIfDisposable();
        
        base.Dispose();
    }
}
