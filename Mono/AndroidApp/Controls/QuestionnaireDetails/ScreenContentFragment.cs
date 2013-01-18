using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails.ScreenItems;
using AndroidApp.Core;
using AndroidApp.Events;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Java.Interop;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class ScreenContentFragment : AbstractScreenChangingFragment
    {
        private readonly IQuestionViewFactory questionViewFactory;

        protected ScreenContentFragment()
        {
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.RetainInstance = true;
            this.questionViewFactory = new DefaultQuestionViewFactory();
        }

        public ScreenContentFragment(QuestionnaireScreenViewModel model)
            : this()
        {
           
            this.Model = model;
            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no
                // reason to create our view.
                return null;
            }
            ScrollView sv = new ScrollView(inflater.Context);
            sv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            LinearLayout ll = new LinearLayout(inflater.Context);
            ll.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            ll.Orientation = Orientation.Vertical;
            ll.SetPadding(0, 10, 0, 0);

            sv.AddView(ll);
            foreach (var item in Model.Items)
            {
                var question = item as QuestionViewModel;
                View itemView = null;
                if (question != null)
                {
                    itemView = this.questionViewFactory.CreateQuestionView(inflater.Context, question);
                }
                var group = item as GroupViewModel;
                if (group != null)
                {
                    var groupView = new GroupView(inflater.Context, group);
                    groupView.ScreenChanged += new EventHandler<ScreenChangedEventArgs>(groupView_ScreenChanged);
                    itemView = groupView;
                }
                if (itemView != null)
                    ll.AddView(itemView);
            }
            return sv;
            /*inflater.Inflate(Resource.Layout.ScreenNavigationView, null);
            this.Container.ItemClick += new EventHandler<AdapterView.ItemClickEventArgs>(Container_ItemClick);*/
            //  return retval;
        }
       
        private void groupView_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            OnScreenChanged(e);
        }


        public QuestionnaireScreenViewModel Model { get; private set; }

    }
}