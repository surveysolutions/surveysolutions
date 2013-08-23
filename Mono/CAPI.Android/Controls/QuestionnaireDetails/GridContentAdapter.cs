using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Controls.QuestionnaireDetails.Roster;
using CAPI.Android.Core;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Events;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class GridContentAdapter : SmartAdapter<RosterRow>
    {
        private readonly Context context;
        private readonly Action<ScreenChangedEventArgs> OnScreenChanged;
        private readonly EventHandler<RosterItemClickEventArgs> RosterItemsClick;

        public GridContentAdapter(IList<RosterRow> items, Context context,
                                  Action<ScreenChangedEventArgs> onScreenChanged,
                                  EventHandler<RosterItemClickEventArgs> rosterItemsClick)
            : base(items)
        {
            this.context = context;
            this.OnScreenChanged = onScreenChanged;
            this.RosterItemsClick = rosterItemsClick;
        }

        protected override View BuildViewItem(RosterRow dataItem, int position)
        {
            LinearLayout view = new LinearLayout(context);
            view.Orientation = Orientation.Horizontal;
            view.LayoutParameters = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                            ViewGroup.LayoutParams.FillParent, 1);

            if (!dataItem.Enabled)
                view.Visibility = ViewStates.Gone;

            Button first = new Button(context);
            first.SetTag(Resource.Id.PrpagationKey, dataItem.Id.ToString());
            first.Click += first_Click;
            first.Text = dataItem.ScreenName;
            AlignTableCell(first);
            /*    IList<PropertyChangedEventHandler> handlers;

                if (!rowEventHandlers.ContainsKey(dataItem.Id))
                {
                    handlers = new List<PropertyChangedEventHandler>();
                    rowEventHandlers.Add(dataItem.Id, handlers);
                }
                else
                    handlers = rowEventHandlers[dataItem.Id];*/

            /*   PropertyChangedEventHandler handler = new StatusChangedHandlerClosure(view, Model, first, tvEmptyLabelDescription, llTablesContainer).StatusChangedHandler;
               handlers.Add(handler);
               rosterItem.PropertyChanged += handler;*/

            AlignTableCell(first);
            view.AddView(first);

            for (int i = 0; i < dataItem.Items.Count; i++)
            {

                QuestionViewModel rowModel = dataItem.Items[i] as QuestionViewModel;
                RosterQuestionView rosterCell = new RosterQuestionView(context, rowModel);
                rosterCell.RosterItemsClick += RosterItemsClick;
            //    rosterQuestionViews.Add(rosterCell);

                AlignTableCell(rosterCell);
                view.AddView(rosterCell);
            }
            return view;
        }

        private void AlignTableCell(View view)
        {
            view.LayoutParameters = new LinearLayout.LayoutParams(0,
                                                                  ViewGroup.LayoutParams.WrapContent, 1);
        }

        private void first_Click(object sender, EventArgs e)
        {
            var senderButton = sender as Button;
            if (senderButton == null)
                return;
            var publicKey = ItemPublicKey.Parse(senderButton.GetTag(Resource.Id.PrpagationKey).ToString());
            OnScreenChanged(new ScreenChangedEventArgs(publicKey));

        }
    }
}