using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AndroidApp.ViewModel.QuestionnaireDetails.GridItems
{
    public class SelectableHeaderItem:HeaderItem
    {
        public SelectableHeaderItem(Guid publicKey, string title, string instructions, IEnumerable<AnswerViewModel> answers)
            : base(publicKey, title, instructions)
        {
            this.Answers = answers;
        }
        public IEnumerable<AnswerViewModel> Answers { get; private set; }
    }
}