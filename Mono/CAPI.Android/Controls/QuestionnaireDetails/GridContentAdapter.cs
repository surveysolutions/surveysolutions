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
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core;
using CAPI.Android.Events;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    public class GridContentAdapter : SmartAdapter<RosterTable>
    {
        private readonly Context context;
        private readonly Action<ScreenChangedEventArgs> onScreenChanged;
        private readonly int columnCount;
       
        private readonly Guid questionnaireId;

        public GridContentAdapter(QuestionnaireGridViewModel model,int columnCount, Context context,
                                  Action<ScreenChangedEventArgs> onScreenChanged)
            : base(CreateItemList(model, columnCount))
        {
            this.context = context;
            this.columnCount = columnCount;
            this.onScreenChanged = onScreenChanged;
            this.questionnaireId = model.QuestionnaireId;
        }

        private static IList<RosterTable> CreateItemList(QuestionnaireGridViewModel model, int columnCount)
        {
            var result = new List<RosterTable>();
            for (int i = 0; i < model.Header.Count; i = i + columnCount)
            {
                result.Add(new RosterTable(model.Header.Skip(i).Take(columnCount).ToList(),model.Rows));
            }
            return result;
        }

        protected override View BuildViewItem(RosterTable dataItem, int position)
        {
            var view = new PartOfRosterView(context,dataItem,columnCount,questionnaireId,onScreenChanged);
            
            var llTableParentLayoutParameters = new ListView.LayoutParams(ViewGroup.LayoutParams.FillParent,
                                                             ViewGroup.LayoutParams.WrapContent);
            view.LayoutParameters = llTableParentLayoutParameters;
            
            return view;
        }
    }
}