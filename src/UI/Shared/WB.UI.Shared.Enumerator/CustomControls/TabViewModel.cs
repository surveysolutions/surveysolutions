using MvvmCross.ViewModels;

namespace WB.UI.Shared.Enumerator.CustomControls;

public class TabViewModel2 : MvxViewModel
{
    public string Title { get; set; }
    public bool IsEnabled { get; set; }
    public List<MvxViewModel> Items { get; set; }
}
