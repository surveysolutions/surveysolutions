using AndroidX.RecyclerView.Widget;

namespace WB.UI.Shared.Enumerator.CustomControls;

public class NoAnimationItemAnimator : DefaultItemAnimator
{
    public override bool AnimateRemove(RecyclerView.ViewHolder holder)
    {
        DispatchRemoveFinished(holder);
        return false;
    }

    public override bool AnimateAdd(RecyclerView.ViewHolder holder)
    {
        DispatchAddFinished(holder);
        return false;
    }

    public override bool AnimateMove(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
    {
        DispatchMoveFinished(holder);
        return false;
    }

    public override bool AnimateChange(RecyclerView.ViewHolder oldHolder, RecyclerView.ViewHolder newHolder, int fromLeft, int fromTop, int toLeft, int toTop)
    {
        DispatchChangeFinished(oldHolder, true);
        DispatchChangeFinished(newHolder, false);
        return false;
    }

    public override void RunPendingAnimations()
    {

    }

    public override void EndAnimation(RecyclerView.ViewHolder item)
    {
        if (item != null)
        {
            DispatchRemoveFinished(item);
            DispatchAddFinished(item);
            DispatchMoveFinished(item);
            DispatchChangeFinished(item, true);
            DispatchChangeFinished(item, false);
        }
    }

    public override void EndAnimations()
    {
        
    }

    public override bool IsRunning => false;
}
