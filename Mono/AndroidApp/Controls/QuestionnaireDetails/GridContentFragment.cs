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
using AndroidApp.ViewModel.QuestionnaireDetails;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class GridContentFragment : Fragment
    {
        public GridContentFragment(QuestionnaireGridViewModel model)
            : base()
        {
            this.Model = model;
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
            th.AddView(first);

            foreach (HeaderItem headerItem in Model.Header)
            {
                TextView column = new TextView(inflater.Context);
                column.Text = headerItem.Title;
                AssignHeaderStyles(column);
                th.AddView(column);
            }

            tl.AddView(th);
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
                AssignHeaderStyles(first);
                th.AddView(first);

                foreach (RowItem abstractRowItem in rosterItem.RowItems)
                {
                    Button rowViewItem = new Button(inflater.Context);
                    rowViewItem.Text = abstractRowItem.Answer;
                    AssignHeaderStyles(rowViewItem);
                    th.AddView(rowViewItem);
                }


                tl.AddView(th);
            }
        }

        void first_Click(object sender, EventArgs e)
        {
            var senderButton = sender as Button;
            if(senderButton==null)
                return;
            var publicKey = ItemPublicKey.Parse(senderButton.GetTag(Resource.Id.PrpagationKey).ToString());
            OnScreenChanged(new ScreenChangedEventArgs(publicKey));
            
        }
        protected void OnScreenChanged(ScreenChangedEventArgs evt)
        {
            var handler = ScreenChanged;
            if (handler != null)
                handler(this, evt);
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;

        protected void AssignHeaderStyles(TextView tv)
        {
            tv.Gravity = GravityFlags.Center;
            tv.SetPadding(10,10,10,10);
            tv.TextSize = 20;
            tv.SetBackgroundResource(Resource.Drawable.grid_headerItem);
        }

        public QuestionnaireGridViewModel Model { get; private set; }

       
    }
}