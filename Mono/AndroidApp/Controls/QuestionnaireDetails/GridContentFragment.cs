using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails.Roster;
using AndroidApp.Controls.QuestionnaireDetails.ScreenItems;
using AndroidApp.Core;
using AndroidApp.Events;
using AndroidApp.ViewModel.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Java.Interop;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class GridContentFragment : AbstractScreenChangingFragment
    {
        public GridContentFragment(QuestionnaireGridViewModel model)
            : this()
        {
            
            this.Model = model;
        }
        protected GridContentFragment()
            : base()
        {
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.RetainInstance = true;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
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

            var breadcrumbs = new BreadcrumbsView(inflater.Context, Model.Breadcrumbs, OnScreenChanged);
            breadcrumbs.SetPadding(0, 0, 0, 10);
            ll.AddView(breadcrumbs);

            TableLayout tl = new TableLayout(inflater.Context);
            tl.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);
            tl.StretchAllColumns = true;
            

            CreateHeader(inflater, tl);
            CreateBody(inflater, tl);
            ll.AddView(tl);
            sv.AddView(ll);
            return sv;
        }
        protected void CreateHeader(LayoutInflater inflater, TableLayout tl)
        {
            TableRow th = new TableRow(inflater.Context);
            TextView first = new TextView(inflater.Context);
            AssignHeaderStyles(first);
            first.SetBackgroundResource(Resource.Drawable.grid_headerItem);
            th.AddView(first);

            foreach (HeaderItem headerItem in Model.Header)
            {
                TextView column = new TextView(inflater.Context);
                column.Text = headerItem.Title;
                if (!string.IsNullOrEmpty(headerItem.Instructions))
                {

                    var img = inflater.Context.Resources.GetDrawable(Android.Resource.Drawable.IcDialogInfo);
                    //img.SetBounds(0, 0, 45, 45);
                    column.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
                    column.Click += new EventHandler(column_Click);
                   
                }
                column.SetTag(Resource.Id.Index, Model.Header.IndexOf(headerItem));
                AssignHeaderStyles(column);
                column.SetBackgroundResource(Resource.Drawable.grid_headerItem);
                th.AddView(column);
            }

            tl.AddView(th);
        }

        void column_Click(object sender, EventArgs e)
        {
            int i = int.Parse(((TextView) sender).GetTag(Resource.Id.Index).ToString());
            var instructionsBuilder = new AlertDialog.Builder(this.Activity);
            instructionsBuilder.SetMessage(Model.Header[i].Instructions);
            
            instructionsBuilder.Show();

            
        }
        protected void CreateBody(LayoutInflater inflater, TableLayout tl)
        {
            foreach (RosterItem rosterItem in Model.Rows)
            {
                TableRow th = new TableRow(inflater.Context);
                Button first = new Button(inflater.Context);
                first.SetTag(Resource.Id.PrpagationKey, rosterItem.PublicKey.ToString());
                first.Click += new EventHandler(first_Click);
                first.Text = rosterItem.Title;
                // AssignHeaderStyles(first);
                th.AddView(first);

                foreach (var abstractRowItem in rosterItem.RowItems)
                {
                    RosterQuestionView rowViewItem = new RosterQuestionView(inflater.Context,
                                                                            inflater.Context as IMvxBindingActivity,
                                                                            abstractRowItem as QuestionViewModel);
                    rowViewItem.RosterItemsClick += rowViewItem_RosterItemsClick;
                   // AssignHeaderStyles(rowViewItem);
                    /*  Button rowViewItem = new Button(inflater.Context);
                      rowViewItem.Text = abstractRowItem.Answer;
                    
                      rowViewItem.SetBackgroundResource(Resource.Drawable.grid_headerItem);

                      rowViewItem.Enabled = abstractRowItem.Enabled;

                      if (abstractRowItem.Enabled)
                      {
                          rowViewItem.Click += new EventHandler(rowViewItem_Click);
                          if (!abstractRowItem.Valid)
                              rowViewItem.SetBackgroundResource(Resource.Drawable.questionInvalidShape);
                          else if (abstractRowItem.Answered)
                              rowViewItem.SetBackgroundResource(Resource.Drawable.questionAnsweredShape);
                      }

                      rowViewItem.SetTag(Resource.Id.Index, rosterItem.RowItems.IndexOf(abstractRowItem));
                      rowViewItem.SetTag(Resource.Id.PrpagationKey, abstractRowItem.PropagationKey.ToString());*/
                    th.AddView(rowViewItem);
                }


                tl.AddView(th);
            }
        }

        void rowViewItem_RosterItemsClick(object sender, RosterItemClickEventArgs e)
        {
            /*   var headerItem = this.Model.Header.FirstOrDefault(h => h.PublicKey == e.Model.PublicKey.PublicKey);
               if (headerItem == null)
                   return;*/

            var setAnswerPopup = new AlertDialog.Builder(this.Activity);
            setAnswerPopup.SetView(this.questionViewFactory.CreateQuestionView(this.Activity, e.Model /*,
                                                                                       headerItem*/));
            //  setAnswerPopup.Show();
            var dialog = setAnswerPopup.Create();

            PropertyChangedEventHandler answerHandler = (s, evt) =>
                {
                    if (evt.PropertyName == "AnswerString")
                    {
                        dialog.Dismiss();
                    }
                };
            
            dialog.DismissEvent += (dialogSender, dialogEvt) =>
                {
                    e.Model.PropertyChanged -= answerHandler;
                };
            dialog.Show();

            e.Model.PropertyChanged += answerHandler;
        }




        void first_Click(object sender, EventArgs e)
        {
            var senderButton = sender as Button;
            if(senderButton==null)
                return;
            var publicKey = ItemPublicKey.Parse(senderButton.GetTag(Resource.Id.PrpagationKey).ToString());
            OnScreenChanged(new ScreenChangedEventArgs(publicKey));
            
        }
       

        protected void AssignHeaderStyles(TextView tv)
        {
       //     tv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);
            tv.Gravity = GravityFlags.Center;
            tv.SetPadding(10,10,10,10);
            tv.TextSize = 20;
            
        }
        protected readonly IQuestionViewFactory questionViewFactory;
        public QuestionnaireGridViewModel Model { get; private set; }

       
    }
}