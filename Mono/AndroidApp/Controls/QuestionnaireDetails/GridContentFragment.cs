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

            var breadcrumbs = new BreadcrumbsView(inflater.Context, Model.Breadcrumbs, OnScreenChanged);
            breadcrumbs.SetPadding(0, 0, 0, 10);
            ll.AddView(breadcrumbs);
            for (int i = 0; i < Model.Header.Count; i = i + 2)
            {
                var count = Math.Min(Model.Header.Count - i, 2);
                BuildTable(inflater.Context, ll, i, count);
            }
            sv.AddView(ll);
            return sv;
        }

        protected void BuildTable(Context context, LinearLayout ll, int index, int count)
        {
            TableLayout tl = new TableLayout(context);
            tl.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            tl.StretchAllColumns = true;

            CreateHeader(context, tl, index,count);
            CreateBody(context, tl , index,count);


            ll.AddView(tl);
        }

        protected void CreateHeader(Context context, TableLayout tl, int index, int count)
        {
            TableRow th = new TableRow(context);
            TextView first = new TextView(context);
            AssignHeaderStyles(first);
            first.SetBackgroundResource(Resource.Drawable.grid_headerItem);
            th.AddView(first);

            foreach (HeaderItem headerItem in Model.Header.Skip(index).Take(count))
            {
                TextView column = new TextView(context);
                column.Text = headerItem.Title;
                if (!string.IsNullOrEmpty(headerItem.Instructions))
                {

                    var img = context.Resources.GetDrawable(Android.Resource.Drawable.IcDialogInfo);
                    //img.SetBounds(0, 0, 45, 45);
                    column.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
                    column.Click += new EventHandler(column_Click);
                   
                }
                column.SetTag(Resource.Id.ScreenId, headerItem.PublicKey.ToString());
                AssignHeaderStyles(column);
                column.SetBackgroundResource(Resource.Drawable.grid_headerItem);
                th.AddView(column);
            }

            tl.AddView(th);
        }


        protected void CreateBody(Context context, TableLayout tl, int index, int count)
        {
            foreach (var rosterItem in Model.Rows)
            {
                TableRow th = new TableRow(context);
                Button first = new Button(context);
                first.SetTag(Resource.Id.PrpagationKey, rosterItem.ScreenId.ToString());
                first.Click += new EventHandler(first_Click);
                first.Text = rosterItem.ScreenName;
                // AssignHeaderStyles(first);
                th.AddView(first);

                foreach (var abstractRowItem in rosterItem.Items.Skip(index).Take(count))
                {
                    RosterQuestionView rowViewItem = new RosterQuestionView(context,
                                                                            context as IMvxBindingActivity,
                                                                            abstractRowItem as QuestionViewModel);
                    rowViewItem.RosterItemsClick += rowViewItem_RosterItemsClick;
                    th.AddView(rowViewItem);
                }


                tl.AddView(th);
            }
        }

        void column_Click(object sender, EventArgs e)
        {
            var key = Guid.Parse(((TextView) sender).GetTag(Resource.Id.ScreenId).ToString());
            var instructionsBuilder = new AlertDialog.Builder(this.Activity);
            instructionsBuilder.SetMessage(Model.Header.First(h=>h.PublicKey==key).Instructions);
            
            instructionsBuilder.Show();

            
        }
        void rowViewItem_RosterItemsClick(object sender, RosterItemClickEventArgs e)
        {
            /*   var headerItem = this.Model.Header.FirstOrDefault(h => h.PublicKey == e.Model.PublicKey.PublicKey);
               if (headerItem == null)
                   return;*/

            var setAnswerPopup = new AlertDialog.Builder(this.Activity);
            setAnswerPopup.SetView(this.questionViewFactory.CreateQuestionView(this.Activity, e.Model,
                                                                               Model.QuestionnaireId));
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