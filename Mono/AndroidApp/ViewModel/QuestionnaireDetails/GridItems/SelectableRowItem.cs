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
using Main.Core.Entities.SubEntities;

namespace AndroidApp.ViewModel.QuestionnaireDetails.GridItems
{
    public class SelectableRowItem : AbstractQuestionRowItem
    {
        public SelectableRowItem(Guid publicKey, Guid propagationKey, QuestionType questionType,
                                 bool enabled, bool valid, string comments, string answer, IEnumerable<AnswerViewModel> answers)
            : base(publicKey, propagationKey,enabled, comments, questionType, valid, answer)
        {
            Answers = answers;
        }
        public IEnumerable<AnswerViewModel> Answers { get; private set; }
    }
}