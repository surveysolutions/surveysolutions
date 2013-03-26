using System;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class QuestionnaireNavigationFragment : ListFragment, IScreenChanging
    {
        #region public fields
        public QuestionnaireNavigationFragment()
        {
            this.RetainInstance = true;
        }
        public QuestionnaireNavigationFragment(CompleteQuestionnaireView model):this()
        {
            this.RetainInstance = true;
            this.Model = model;
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
        public CompleteQuestionnaireView Model { get;  set; }
        public int SelectedIndex
        {
            get { return selectedItemIndex; }
        }
        private int selectedItemIndex=0;
        private QuestionnaireNavigationAdapter adapter;
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
            adapter = new QuestionnaireNavigationAdapter(this.Activity, Model, selectedItemIndex);
            this.ListAdapter = adapter;

        }

        public void SelectItem(int ind)
        {
            if(selectedItemIndex==ind)
                return;
            selectedItemIndex = ind;
            adapter.SelectItem(selectedItemIndex);
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