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
    public abstract class AbstractRowItem
    {
        public AbstractRowItem(Guid publicKey, Guid propagationKey, string text, bool enabled, string comments)
        {
            PublicKey = new ItemPublicKey(publicKey,propagationKey);
            Text = text;
        //   
            Enabled = enabled;
            Comments = comments;
          //  this.Answer = answer;
         /*   bool answered = !string.IsNullOrEmpty(answer);

          */
        }

        public ItemPublicKey PublicKey { get; private set; }
   //     public string Text { get; private set; }
      //  
        public bool Enabled { get; private set; }
        public string Comments { get; private set; }
        public string Text { get; protected set; }

     //  
    }

    public abstract class AbstractQuestionRowItem : AbstractRowItem
    {
        protected AbstractQuestionRowItem(Guid publicKey, Guid propagationKey,  bool enabled, string comments, QuestionType questionType, bool valid, string answer)
            : base(publicKey, propagationKey, answer, enabled, comments)
        {
            QuestionType = questionType;
            if (enabled)
            {
                Status = Status | QuestionStatus.Enabled;
            }
            if (valid)
            {
                Status = Status | QuestionStatus.Valid;
            }
            bool answered = !string.IsNullOrEmpty(answer);
            if (answered)
            {
                Status = Status | QuestionStatus.Answered;
            }
        }
      //  public string AnswerString { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public QuestionStatus Status { get; protected set; }
    }
}