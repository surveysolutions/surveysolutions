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
    public class QuestionnaireNavigationView : LinearLayout
    {
        public Guid QuestionnaireId
        {
            get { return questionnaireId; }
            set
            {
                questionnaireId = value;
               /* this.RemoveAllViews();*/
                var questionnaireData =
                    CapiApplication.LoadView<QuestionnaireNavigationPanelInput, QuestionnaireNavigationPanelModel>(
                        new QuestionnaireNavigationPanelInput(this.QuestionnaireId));
                this.Container.Adapter = new QuestionnaireNavigationAdapter(this.Context, questionnaireData.Items);
            }
        }

        protected ListView Container
        {
            get { return FindViewById<ListView>(Resource.Id.llScreen); }
        }
        public event EventHandler<ScreenChangedEventArgs> ItemClick;
        protected void OnItemClick(Guid groupKey)
        {
            var handler = ItemClick;
            if (handler != null)
                handler(this, new ScreenChangedEventArgs(groupKey));
        }

        private Guid questionnaireId;
        public QuestionnaireNavigationView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(context);
        }

        public QuestionnaireNavigationView(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(context);
        }
        private void Initialize(Context context)
        {

            LayoutInflater layoutInflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
            layoutInflater.Inflate(Resource.Layout.ScreenNavigationView, this);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);
        }

        void Container_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = e.View;
            var screenId = Guid.Parse(item.GetTag(Resource.Id.ScreenId).ToString());
            OnItemClick(screenId);
        }


  /*      private void Initialize(IAttributeSet attrs)
        {
            this.ItemClick += new EventHandler<ItemClickEventArgs>(QuestionnaireNavigationView_ItemClick);
        }

        void QuestionnaireNavigationView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = e.View;
            var screenId = Guid.Parse(item.GetTag(Resource.Id.ScreenId).ToString());
        }*/


       
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