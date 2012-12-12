using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
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
        public Guid QuestionnaireId { get; set; }


        #endregion



        protected void OnItemClick(Guid groupKey)
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

            var questionnaireData =
                CapiApplication.LoadView<QuestionnaireNavigationPanelInput, QuestionnaireNavigationPanelModel>(
                    new QuestionnaireNavigationPanelInput(this.QuestionnaireId));

            this.ListAdapter = new QuestionnaireNavigationAdapter(this.Activity, questionnaireData.Items);
          
        }


       
        public override void OnListItemClick(ListView l, View v, int pos, long id)
        {
            ListView.SetItemChecked(pos, true);
            var screenId = Guid.Parse(v.GetTag(Resource.Id.ScreenId).ToString());
            OnItemClick(screenId);
        }

    }

    public class ScreenChangedEventArgs : EventArgs
    {
        public ScreenChangedEventArgs(Guid screenId)
        {
            ScreenId = screenId;
        }

        public Guid ScreenId { get; private set; }
    }

}