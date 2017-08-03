using Android.Support.V7.Widget;
using WB.UI.Shared.Enumerator.CustomBindings;

namespace WB.UI.Interviewer.CustomBindings
{
    public class RecyclerViewScrollToPositionBinding : BaseBinding<RecyclerView, int?>
    {
        public RecyclerViewScrollToPositionBinding(RecyclerView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(RecyclerView control, int? value)
        {
            
            if (value.HasValue)
            {
                control.ScrollToPosition(value.Value);
            }
        }
    }
}