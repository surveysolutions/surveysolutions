using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationFragment : ListFragment
    {
       


        #region public fields

        public event EventHandler<ScreenChangedEventArgs> ItemClick;
        public IEnumerable<QuestionnaireNavigationPanelItem> DataItems { get; set; }

        #endregion



        protected void OnItemClick(ItemPublicKey? groupKey)
        {
            var handler = ItemClick;
            if (handler != null)
                handler(this, new ScreenChangedEventArgs(groupKey));
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var result=  base.OnCreateView(inflater, container, savedInstanceState);
           
            
         //   result.LayoutParameters = new ViewGroup.LayoutParams(10, ViewGroup.LayoutParams.FillParent);
            return result;
        }
        public override void OnActivityCreated(Bundle savedInstanceState)
        {

            base.OnActivityCreated(savedInstanceState);
            
            this.ListView.ChoiceMode = ChoiceMode.Single;
         //   this.ListView.SetSelector(Resource.Drawable.navigation_Selector);
            this.ListAdapter = new QuestionnaireNavigationAdapter(this.Activity, DataItems);
          
        }

    
        

        public void SelectItem(int ind)
        {
            for (int idx = 0; idx < this.ListView.ChildCount; idx++)
            {
                if (idx == ind)
                    this.ListView.GetChildAt(idx).SetBackgroundColor(Color.Blue);
                else
                    this.ListView.GetChildAt(idx).SetBackgroundColor(Color.Transparent);
                //  EnableDisableView(group.GetChildAt(idx), enabled);
            }
        }

        public override void OnListItemClick(ListView l, View v, int pos, long id)
        {
            //  ListView.SetItemChecked(pos, true);
            // v.Selected = true;
           // v.SetBackgroundColor(Color.Green);
            SelectItem(pos);
            var tag = v.GetTag(Resource.Id.ScreenId);
            ItemPublicKey? screenId = null;
            if (tag != null)
            {
                screenId = ItemPublicKey.Parse(v.GetTag(Resource.Id.ScreenId).ToString());
            }
            OnItemClick(screenId);
        }


    }

    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangedEventArgs(ItemPublicKey? screenId)
        {
            ScreenId = screenId;
        }
        public ItemPublicKey? ScreenId { get; private set; }
    }

}