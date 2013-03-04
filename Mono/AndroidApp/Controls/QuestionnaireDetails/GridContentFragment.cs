using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidApp.Controls.QuestionnaireDetails.Roster;
using AndroidApp.Controls.QuestionnaireDetails.ScreenItems;
using AndroidApp.Core;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails.GridItems;
using AndroidApp.Events;
using AndroidApp.Extensions;
using Cirrious.MvvmCross.Binding.Droid.Interfaces.Views;
using Java.Interop;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    public class GridContentFragment : AbstractScreenChangingFragment
    {
        private readonly CompleteQuestionnaireView questionnaire;
        protected TextView tvEmptyLabelDescription;
        protected LinearLayout llTablesContainer;
        protected LinearLayout top;
        protected Dictionary<ItemPublicKey,IList<  PropertyChangedEventHandler>> rowEventHandlers;
        protected List<RosterQuestionView>  rosterQuestionViews=new List<RosterQuestionView>();
        protected AnswerSetPopupClosure answerHandler;
        public GridContentFragment(QuestionnaireGridViewModel model, CompleteQuestionnaireView questionnaire)
            : this()
        {
           
            this.Model = model;
            this.questionnaire = questionnaire;
        }
        public GridContentFragment()
            : base()
        {
            this.rowEventHandlers = new Dictionary<ItemPublicKey, IList<PropertyChangedEventHandler>>();
            this.rosterQuestionViews=new List<RosterQuestionView>();
            this.questionViewFactory = new DefaultQuestionViewFactory();
            this.RetainInstance = true;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (container == null)
            {
                // Currently in a layout without a container, so no
                // reason to create our view.
                return null;
            }
            top = new LinearLayout(inflater.Context);
            top.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                              ViewGroup.LayoutParams.FillParent);
            top.Orientation = Orientation.Vertical;
            var breadcrumbs = new BreadcrumbsView(inflater.Context,
                                                  questionnaire.RestoreBreadCrumbs(Model.Breadcrumbs).ToList(),
                                                  OnScreenChanged);
            breadcrumbs.SetPadding(0, 0, 0, 10);
            top.AddView(breadcrumbs);

            ScrollView sv = new ScrollView(inflater.Context);
            sv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            LinearLayout ll = new LinearLayout(inflater.Context);
            ll.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            ll.Orientation = Orientation.Vertical;

            BuildEmptyLabelDescription(inflater.Context, ll);
            BuildTabels(inflater.Context, ll);
            sv.AddView(ll);
            sv.EnableDisableView(!SurveyStatus.IsStatusAllowCapiSync(questionnaire.Status));
            top.AddView(sv);
            return top;
        }
        public override void OnDetach()
        {
            base.OnDetach();
            foreach (var row in Model.Rows)
            {
                var handlers = rowEventHandlers[row.ScreenId];
                foreach (PropertyChangedEventHandler propertyChangedEventHandler in handlers)
                {
                    row.PropertyChanged -= propertyChangedEventHandler;
                }

            }
            foreach (RosterQuestionView rosterQuestionView in rosterQuestionViews)
            {
                rosterQuestionView.Dispose();
            }
            rosterQuestionViews = new List<RosterQuestionView>();
            if (answerHandler != null)
            {
                answerHandler.Dispose();
                answerHandler = null;
            }
        }

        protected void BuildEmptyLabelDescription(Context context, LinearLayout ll)
        {
            tvEmptyLabelDescription = new TextView(context);
            tvEmptyLabelDescription.Gravity = GravityFlags.Center;
            tvEmptyLabelDescription.TextSize = 22;
            tvEmptyLabelDescription.SetPadding(10, 10, 10, 10);
            tvEmptyLabelDescription.Text = "Questions are absent";
            tvEmptyLabelDescription.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            if (Model.Rows.Any(r => r.Enabled))
                tvEmptyLabelDescription.Visibility=ViewStates.Gone;
            ll.AddView(tvEmptyLabelDescription);
        }
        protected void BuildTabels(Context context, LinearLayout ll)
        {
            llTablesContainer = new LinearLayout(context);
            llTablesContainer.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.FillParent);
            llTablesContainer.Orientation = Orientation.Vertical;
            const int columnCount = 2;
            for (int i = 0; i < Model.Header.Count; i = i + columnCount)
            {
                BuildTable(context, llTablesContainer, i, columnCount);
            }
            if (!Model.Rows.Any(r => r.Enabled))
                llTablesContainer.Visibility = ViewStates.Gone;
            ll.AddView(llTablesContainer);
        }

        protected void BuildTable(Context context, LinearLayout ll, int index, int count)
        {
            TableLayout tl = new TableLayout(context);
            
            var layout= new TableLayout.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            layout.SetMargins(0, 0, 0, 10);
            
            tl.LayoutParameters = layout;

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
            th.AddView(first);
            for (int i = index; i < index+count; i++)
            {
                HeaderItem headerItem = null;
                if (Model.Header.Count > i)
                    headerItem = Model.Header[i];
                TextView column = new TextView(context);
                if (headerItem != null)
                {
                    column.Text = headerItem.Title;
                    if (!string.IsNullOrEmpty(headerItem.Instructions))
                    {

                        var img = context.Resources.GetDrawable(Android.Resource.Drawable.IcDialogInfo);
                        column.SetCompoundDrawablesWithIntrinsicBounds(null, null, img, null);
                        column.Click += new EventHandler(column_Click);
                    }
                    column.SetTag(Resource.Id.ScreenId, headerItem.PublicKey.ToString());
                   
                }

                AssignHeaderStyles(column);
                th.AddView(column);
            }

            tl.AddView(th);
        }


        protected void CreateBody(Context context, TableLayout tl, int index, int count)
        {
            foreach (var rosterItem in Model.Rows)
            {
               
                TableRow th = new TableRow(context);
                if (!rosterItem.Enabled)
                    th.Visibility = ViewStates.Gone;
               
                Button first = new Button(context);
                first.SetTag(Resource.Id.PrpagationKey, rosterItem.ScreenId.ToString());
                first.Click += new EventHandler(first_Click);
                first.Text = rosterItem.ScreenName;
                IList<PropertyChangedEventHandler> handlers;

                if(!rowEventHandlers.ContainsKey(rosterItem.ScreenId))
                {
                    handlers = new List<PropertyChangedEventHandler>();
                    rowEventHandlers.Add(rosterItem.ScreenId,handlers);
                }
                else
                    handlers = rowEventHandlers[rosterItem.ScreenId];
                PropertyChangedEventHandler handler = new StatusChangedHandlerClosure(th, Model, first, tvEmptyLabelDescription, llTablesContainer).StatusChangedHandler;
                handlers.Add(handler);
                rosterItem.PropertyChanged += handler;
                AlignTableCell(first);
                th.AddView(first);

                //foreach (var abstractRowItem in rosterItem.Items.Skip(index).Take(count))
                //{
                for (int i = index; i < index+count; i++)
                {
                    View rosterCell;
                    if (i < rosterItem.Items.Count())
                    {
                        QuestionViewModel rowModel = rosterItem.Items[i] as QuestionViewModel;
                        RosterQuestionView rowViewItem = new RosterQuestionView(context,
                                                                                context as IMvxBindingActivity, rowModel);
                        rowViewItem.RosterItemsClick += rowViewItem_RosterItemsClick;
                        rosterQuestionViews.Add(rowViewItem);
                        rosterCell = rowViewItem;
                    }
                    else
                    {
                        rosterCell = new TextView(context);
                    }
                    AlignTableCell(rosterCell);
                    th.AddView(rosterCell);
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

        private void rowViewItem_RosterItemsClick(object sender, RosterItemClickEventArgs e)
        {
            var group = Model.Rows.FirstOrDefault(r => r.ScreenId.PropagationKey == e.Model.PublicKey.PropagationKey);
            if (group == null)
                return;
            var setAnswerPopup = new AlertDialog.Builder(this.Activity);
            setAnswerPopup.SetView(new RosterItemDialog(this.Activity, e.Model, group.ScreenName, Model.QuestionnaireId,
                                                        questionViewFactory));
            var dialog = setAnswerPopup.Create();

            answerHandler = new AnswerSetPopupClosure(dialog, e.Model);
            dialog.DismissEvent += (dialogSender, dialogEvt) =>
                {
                    if (answerHandler == null)
                        return;
                    answerHandler.Dispose();
                    answerHandler = null;
                };
            dialog.Show();
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
         //   tv.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.FillParent);
         /*   var layout = new TableLayout.LayoutParams(
                                    ViewGroup.LayoutParams.WrapContent,
                                    ViewGroup.LayoutParams.WrapContent);
            layout.Weight =(float)0.3;
            tv.LayoutParameters = layout;*/
            tv.Gravity = GravityFlags.Center;

            tv.SetPadding(10,10,10,10);
            tv.TextSize = 20;
            tv.SetBackgroundResource(Resource.Drawable.grid_headerItem);
            AlignTableCell(tv);
            
        }
       
        protected void AlignTableCell(View view)
        {

            var layout = new TableRow.LayoutParams(0,
                                       ViewGroup.LayoutParams.FillParent);
            
           // layout.Weight =1;
            view.LayoutParameters = layout;
        }

        protected readonly IQuestionViewFactory questionViewFactory;
        public QuestionnaireGridViewModel Model { get; private set; }

        protected class AnswerSetPopupClosure:IDisposable
        {
            private AlertDialog dialog;
            private QuestionViewModel question;
            public AnswerSetPopupClosure(AlertDialog dialog, QuestionViewModel question)
            {
                this.dialog = dialog;
                this.question = question;
                question.PropertyChanged += AnswerSetHandler;
            }
            
            public void AnswerSetHandler(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "AnswerString")
                {
                    dialog.Dismiss();
                    dialog.Dispose();
                }
            }

            public void Dispose()
            {
                question.PropertyChanged -= AnswerSetHandler;
            }
        }

        protected class StatusChangedHandlerClosure
        {
            private readonly TableRow th;
            private readonly QuestionnaireGridViewModel model;
            private readonly Button first;
            private readonly TextView tvEmptyLabelDescription;
            private readonly LinearLayout llTablesContainer;

            public StatusChangedHandlerClosure(TableRow th, QuestionnaireGridViewModel model, Button first, TextView tvEmptyLabelDescription, LinearLayout llTablesContainer)
            {
                this.th = th;
                this.model = model;
                this.first = first;
                this.tvEmptyLabelDescription = tvEmptyLabelDescription;
                this.llTablesContainer = llTablesContainer;
            }

            public void StatusChangedHandler(object sender, PropertyChangedEventArgs e)
            {

                var item = sender as QuestionnaireScreenViewModel;
                if (item == null)
                    return;
                if (e.PropertyName == "Enabled")
                {

                    var visibility = item.Enabled ? ViewStates.Visible : ViewStates.Gone;
                    th.Visibility = visibility;
                    var tableVisible = model.Rows.Any(r => r.Enabled);
                    llTablesContainer.Visibility = tableVisible ? ViewStates.Visible : ViewStates.Gone;
                    tvEmptyLabelDescription.Visibility = !tableVisible ? ViewStates.Visible : ViewStates.Gone;
                    return;
                }
                if (e.PropertyName == "ScreenName")
                {
                    first.Text = item.ScreenName;
                }
            }
        }
    }
}
