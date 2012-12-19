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
            PublicKey = publicKey;
            PropagationKey = propagationKey;
            Text = text;
            QuestionType = questionType;
            Enabled = enabled;
            Valid = valid;
            Comments = comments;
            this.Answer = answer;
            this.Answered = !string.IsNullOrEmpty(answer);
        }

        public Guid PublicKey { get; private set; }
        public Guid PropagationKey { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public bool Enabled { get; private set; }
        public bool Valid { get; private set; }
        public string Comments { get; private set; }
        public bool Answered { get; private set; }
        public string Answer { get; private set; }
    }
}