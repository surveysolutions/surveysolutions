using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.DroidX.RecyclerView.ItemTemplates;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.UI.Shared.Enumerator.CustomControls;

public class CompleteInformationAdapter : MvxExpandableListAdapter
{
    public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
    {
        var view = base.GetGroupView(groupPosition, isExpanded, convertView, parent);

        var hasChildren = GetChildrenCount(groupPosition) != 0;
        
        if (parent is ExpandableListView listView)
        {
            var indicator = (ImageView)view.FindViewById(Android.Resource.Id.Icon);
            if (indicator != null)
            {
                var newVisibilityState = hasChildren ? ViewStates.Visible : ViewStates.Invisible;
                if (indicator.Visibility != newVisibilityState)
                    indicator.Visibility = newVisibilityState;
            }

            /*var btnIndicator = (Button)view.FindViewById(Resource.Id.Complete_Group_Expand_Btn);
            if (btnIndicator != null)
            {
                btnIndicator.Click += ((sender, args) =>
                {
                    //var group = GetGroup(groupPosition) as CompleteGroup;
                    if (listView.IsGroupExpanded(groupPosition))
                    {
                        listView.CollapseGroup(groupPosition);
                        btnIndicator.Text = "+";
                    }
                    else
                    {
                        listView.ExpandGroup(groupPosition);
                        btnIndicator.Text = "-";
                    }
                });
                
                var newVisibilityState = hasChildren ? ViewStates.Visible : ViewStates.Invisible;
                if (btnIndicator.Visibility != newVisibilityState)
                    btnIndicator.Visibility = newVisibilityState;
            }*/
        }

        return view;
    }

    public CompleteInformationAdapter(Context context) : base(context)
    {
    }

    public CompleteInformationAdapter(Context context, IMvxAndroidBindingContext bindingContext) : base(context, bindingContext)
    {
    }

    protected CompleteInformationAdapter(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
    }
}


public class ExpandableAdapter : MvxRecyclerAdapter
{
    public ExpandableAdapter(IMvxAndroidBindingContext bindingContext)
        : base(bindingContext)
    {
    }

    public override int GetItemViewType(int position)
    {
        var item = GetItem(position);
        return item is CompleteGroup ? 0 : 1;
    }

    public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
    {
        if (viewType == 0)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.interview_complete_group_item, parent, false);
            return new MvxRecyclerViewHolder(itemView, BindingContext);
        }
        else
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.interview_complete_child_item, parent, false);
            return new MvxRecyclerViewHolder(itemView, BindingContext);
        }
    }
}

public class ExpandableTemplateSelector : IMvxTemplateSelector
{
    private static readonly Type ParentType = typeof(CompleteGroup);
    private static readonly Type ChildtType = typeof(EntityWithErrorsViewModel);

    public int GetItemViewType(object forItemObject)
    {
        if (forItemObject == null) return -1;

        var typeOfViewModel = forItemObject.GetType();

        if (typeOfViewModel == ParentType)
            return Resource.Layout.interview_complete_group_item;

        if (typeOfViewModel == ChildtType)
            return Resource.Layout.interview_complete_child_item;

        return -1;
    }

    public int GetItemLayoutId(int fromViewType)
    {
        return fromViewType;
    }

    public int ItemTemplateId { get; set; }
}
