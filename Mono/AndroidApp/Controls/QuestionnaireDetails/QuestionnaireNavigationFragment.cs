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
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Events;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationFragment : ListFragment, IScreenChanging
    {
        #region public fields

        public QuestionnaireNavigationFragment()
        {
            this.RetainInstance = true;
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
        public IEnumerable<QuestionnaireScreenViewModel> DataItems { get; set; }
        private int selectedItemIndex=0;
        #endregion

        protected void OnItemClick(ItemPublicKey? groupKey)
        {
            var handler = ScreenChanged;
            if (handler != null)
                handler(this, new ScreenChangedEventArgs(groupKey));
        }

        public override void OnResume()
        {
            base.OnResume();
            this.ListView.ChoiceMode = ChoiceMode.Single;
            this.ListAdapter = new QuestionnaireNavigationAdapter(this.Activity, DataItems, selectedItemIndex);
        }

        public void SelectItem(int ind)
        {
            selectedItemIndex = ind;
            for (int idx = 0; idx < this.ListView.ChildCount; idx++)
            {
                if (idx == ind)
                {
                    this.ListView.GetChildAt(idx).SetBackgroundColor(Color.LightBlue);
                }
                else
                    this.ListView.GetChildAt(idx).SetBackgroundColor(Color.Transparent);
                //  EnableDisableView(group.GetChildAt(idx), enabled);
            }
        }

        public override void OnListItemClick(ListView l, View v, int pos, long id)
        {
            SelectItem(pos);
            var tag = v.GetTag(Resource.Id.ScreenId);
            ItemPublicKey? screenId = null;
            if (tag != null)
            {
                screenId = ItemPublicKey.Parse(v.GetTag(Resource.Id.ScreenId).ToString());
            }
            OnItemClick(screenId);
        }

        public override void OnDetach()
        {
            ScreenChanged = null;
            base.OnDetach();
        }
        public override void OnSaveInstanceState(Bundle p0)
        {
            base.OnSaveInstanceState(p0);
            p0.PutInt("SelectedItem", selectedItemIndex);
        }
        public override void OnViewStateRestored(Bundle p0)
        {
            base.OnViewStateRestored(p0);
            if (p0 != null)
                selectedItemIndex = p0.GetInt("SelectedItem");
        }

    }


}