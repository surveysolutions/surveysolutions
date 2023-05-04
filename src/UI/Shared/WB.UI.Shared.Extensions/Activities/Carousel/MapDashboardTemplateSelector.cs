using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using WB.Core.SharedKernels.Enumerator.ViewModels.Markers;


namespace WB.UI.Shared.Extensions.Activities.Carousel;

public class MapDashboardTemplateSelector : IMvxTemplateSelector
{
    public int GetItemViewType(object forItemObject)
    {
        if (forItemObject == null) return -1;

        if (forItemObject is IMarkerViewModel markerViewModel)
            return Resource.Layout.marker_card;

        return -1;
    }

    public int GetItemLayoutId(int fromViewType)
    {
        return fromViewType;
    }

    public int ItemTemplateId { get; set; }
}