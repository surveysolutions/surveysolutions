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
    public class RowItem
    {
        public RowItem(Guid publicKey, Guid propagationKey, string text, QuestionType questionType, bool enabled, bool valid, string comments, string answer)
        {
            PublicKey = new ItemPublicKey(publicKey,propagationKey);
            Text = text;
            QuestionType = questionType;
            Enabled = enabled;
            Comments = comments;
            this.Answer = answer;
            bool answered = !string.IsNullOrEmpty(answer);

            if (enabled)
            {
                Status = Status | QuestionStatus.Enabled;
            }
            if (valid)
            {
                Status = Status | QuestionStatus.Valid;
            }
            if (answered)
            {
                Status = Status | QuestionStatus.Answered;
            }
        }

        public ItemPublicKey PublicKey { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public bool Enabled { get; private set; }
      //  public bool Valid { get; private set; }
        public string Comments { get; private set; }
      //  public bool Answered { get; private set; }
        public string Answer { get; private set; }

        public QuestionStatus Status { get; protected set; }
    }
}