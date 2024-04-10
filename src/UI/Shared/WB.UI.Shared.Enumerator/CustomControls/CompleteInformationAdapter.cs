using Android.Content;
using Android.Runtime;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels;

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

public class CompleteInformationAdapter2 : MvxExpandableListAdapter
{
    private readonly Context context;

    public override int ItemTemplateId => Resource.Layout.interview_complete_group_item;

    public override int DropDownItemTemplateId => Resource.Layout.interview_complete_child_item;

    IList<CompleteGroupInfo> GroupItems
    {
        get => (IList<CompleteGroupInfo>)ItemsSource;
    }
    
    public CompleteInformationAdapter2(Context context, IMvxAndroidBindingContext bindingContext)
        : base(context, bindingContext)
    {
        this.context = context;
    }

    public override int Count => GroupItems?.Count ?? 0;

    protected override IMvxListItemView CreateBindableView(object dataContext, ViewGroup parent, int templateId)
    {
        return base.CreateBindableView(dataContext, parent, templateId);
    }

    /*public override Java.Lang.Object GetGroup(int groupPosition)
    {
        return GroupItems[groupPosition];
    }*/

    public override long GetGroupId(int groupPosition)
    {
        return groupPosition;
    }

    public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
    {
        var group = GroupAt(groupPosition);
        var view = GetOrCreateViewForBindingContext(group, convertView, parent, Resource.Layout.interview_complete_group_item);
        return view;
    }

    public override int GetChildrenCount(int groupPosition)
    {
        return GroupItems[groupPosition]?.Count ?? 0;
    }

    /*public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
    {
        return GroupItems[groupPosition][childPosition];
    }*/
    
    public CompleteChildInfo ChildAt(int groupPosition, int childPosition)
    {
        return GroupItems.ElementAt(groupPosition).ElementAt(childPosition);
    }
    
    public CompleteGroupInfo GroupAt(int groupPosition)
    {
        return GroupItems.ElementAt(groupPosition);
    }

    public override long GetChildId(int groupPosition, int childPosition)
    {
        return childPosition;
    }

    public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
    {
        var child = ChildAt(groupPosition, childPosition);
        var view = GetOrCreateViewForBindingContext(child, convertView, parent, Resource.Layout.interview_complete_child_item);
        return view;
    }

    public override bool IsChildSelectable(int groupPosition, int childPosition)
    {
        return true;
    }

    private View GetOrCreateViewForBindingContext(object dataContext, View convertView, ViewGroup parent, int templateId)
    {
        if (convertView == null)
        {
            var inflater = (IMvxLayoutInflaterHolder)context.GetSystemService(Context.LayoutInflaterService); /*LayoutInflater*/;
            var bindingContext = new MvxAndroidBindingContext(context, inflater, dataContext);
            convertView = bindingContext.BindingInflate(templateId, parent, false);
        }
        else
        {
            var bindingContext = MvxAndroidBindingContextHelpers.Current();
            bindingContext.DataContext = dataContext;
        }
        return convertView;
    }
}


public class CompleteGroupInfo : MvxObservableCollection<CompleteChildInfo>
{
    public string Title { get; set; }
    //public List<CompleteChildInfo> Children { get; set; }
}

public class CompleteChildInfo : MvxViewModel
{
    public string Title { get; set; }
}
