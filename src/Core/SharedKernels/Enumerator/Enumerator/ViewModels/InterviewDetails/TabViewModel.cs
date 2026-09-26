using System.Collections.Specialized;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

public class TabViewModel : BaseViewModel
{
    public TabViewModel()
    {
        this.items.CollectionChanged += ItemsOnCollectionChanged;
    }
    
    public bool IsEnabled => Items.Count > 0;
    public bool ShowMore => Items.Count < Total;
    public string MoreCount => string.Format(UIResources.Interview_Complete_MoreCountString, Total - Items.Count);
    public string Title { get; set; }
    public CompleteTabContent TabContent { get; set; }
    
    private ObservableRangeCollection<EntityWithErrorsViewModel> items = new();
    public ObservableRangeCollection<EntityWithErrorsViewModel> Items
    {
        get => items;
        set
        {
            if (ReferenceEquals(this.items, value))
                return;
            
            this.items.CollectionChanged -= ItemsOnCollectionChanged;
            this.items = value ?? new ObservableRangeCollection<EntityWithErrorsViewModel>();
            this.items.CollectionChanged += ItemsOnCollectionChanged;
            
            RaisePropertyChanged(() => Items);
            RaiseDependentProperties();
        }
    }
        
    public string Count => Items.Count > 0 ? $"{Total}" : UIResources.Interview_Complete_No;
        
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
                RaiseDependentProperties();
            }
        }
    }
    
    private void ItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) =>
        RaiseDependentProperties();
    
    private void RaiseDependentProperties()
    {
        RaisePropertyChanged(() => IsEnabled);
        RaisePropertyChanged(() => ShowMore);
        RaisePropertyChanged(() => MoreCount);
        RaisePropertyChanged(() => Count);
    }

    public override void Dispose()
    {
        Items.CollectionChanged -= ItemsOnCollectionChanged;
        Items.ForEach(i => i.DisposeIfDisposable());
        Items.DisposeIfDisposable();
        
        base.Dispose();
    }
}
