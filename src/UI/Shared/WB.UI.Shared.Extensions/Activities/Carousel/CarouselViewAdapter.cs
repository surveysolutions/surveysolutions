using Android.Views;
using AndroidX.RecyclerView.Widget;
using AndroidX.Transitions;
using MvvmCross;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.UI.Shared.Enumerator.Activities;
using WB.UI.Shared.Enumerator.Activities.Dashboard;
using Object = Java.Lang.Object;

namespace WB.UI.Shared.Extensions.Activities.Carousel;

public class CarouselViewAdapter : MvxRecyclerAdapter
{
    private readonly IMvxAndroidBindingContext bindingContext;

    public CarouselViewAdapter(IMvxAndroidBindingContext bindingContext)
    {
        this.bindingContext = bindingContext;
    }

    public override AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        var itemBindingContext = new MvxAndroidBindingContext(parent.Context, this.bindingContext.LayoutInflaterHolder);
        var vh = new CarouselViewHolder(this.InflateViewForHolder(parent, viewType, itemBindingContext),
            itemBindingContext);

        return vh;
    }

    public override void OnBindViewHolder(AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder holder, int position)
    {
        base.OnBindViewHolder(holder, position);
        var viewHolder = (CarouselViewHolder)holder;
        viewHolder.GetItem = () => this.GetItem(position);
    }

    public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position, IList<Object> payloads)
    {
        holder.ItemView.Tag = "position-" + position;
        //holder.ItemView.RequestLayout();
        base.OnBindViewHolder(holder, position, payloads);
    }

    protected override void OnItemViewClick(object sender, EventArgs e)
    {
        base.OnItemViewClick(sender, e);
    }
}

public class CarouselViewHolder : MvxRecyclerViewHolder
{
    public ViewGroup DashboardItem { get; }
    public ImageView MenuHandle { get; }

    public CarouselViewHolder(View itemView, IMvxAndroidBindingContext context) : base(itemView, context)
    {
        this.DashboardItem = itemView.FindViewById<ViewGroup>(Resource.Id.dashboardItem);

        if (this.DashboardItem != null)
        {
            this.DashboardItem.Click += CardClick;
        }

        this.MenuHandle = itemView.FindViewById<ImageView>(Resource.Id.menu);

        if (MenuHandle != null)
        {
            this.MenuHandle.Click += MenuClick;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (DashboardItem != null)
            {
                DashboardItem.Click -= CardClick;
                DashboardItem.Dispose();
            }

            if (MenuHandle != null)
            {
                MenuHandle.Click -= MenuClick;
                MenuHandle.Dispose();
            }
        }
    }

    public async void CardClick(object o, EventArgs eventArgs)
    {
        if (GetItem() is IDashboardItem dashboardItemModel)
        {
            var assignmentId = (dashboardItemModel as AssignmentDashboardItemViewModel)?.AssignmentId;
            var interviewId = (dashboardItemModel as InterviewDashboardItemViewModel)?.InterviewId;
            var navigationService = Mvx.IoCProvider.Resolve<IViewModelNavigationService>();
            await navigationService.NavigateToAsync<DashboardItemDetailDialogViewModel, DashboardItemDetailDialogViewModelArgs>(
                new DashboardItemDetailDialogViewModelArgs(
                    interviewId,
                    assignmentId
                ));
            /*DashboardItem.ClearAnimation();

            var transition = new ChangeBounds();
            transition.SetDuration(125);
            TransitionManager.BeginDelayedTransition(DashboardItem, transition);

            dashboardItemModel.IsExpanded = !dashboardItemModel.IsExpanded;*/
        }
    }

    public void MenuClick(object o, EventArgs eventArgs)
    {
        if (GetItem() is IDashboardViewItem dashboardItem)
        {
            var popup = new PopupMenu(this.DashboardItem.Context, this.MenuHandle, GravityFlags.Left);
            var actions = dashboardItem.ContextMenu.Where(a => a.Command.CanExecute());
            foreach (var action in actions)
            {
                var menu = popup.Menu.Add(action.Label);
                action.Tag = menu.GetHashCode();
            }

            popup.MenuItemClick += (s, e) =>
                {
                    var action = dashboardItem.ContextMenu.SingleOrDefault(a => a.Tag == e.Item.GetHashCode());
                    action?.Command?.Execute();
                };

            popup.Show();
        }
    }
    
    public Func<object> GetItem { get; set; }
}