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
using AndroidApp.Converters;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Main.Core.Entities.SubEntities;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{


    public abstract class QuestionViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireItemViewModel
    {
      /*  public QuestionViewModel(AbstractQuestionRowItem rosterItem, HeaderItem headerItem)
            : this(rosterItem.PublicKey, rosterItem.Text, rosterItem.QuestionType, rosterItem.Enabled, headerItem.Instructions, rosterItem.Comments, false, false)
        {
            this.Status = rosterItem.Status;
        }*/

        public QuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, bool enabled, string instructions, string comments, bool valid, bool mandatory, string answerString)
        {
            PublicKey = publicKey;
            Text = text;
            QuestionType = type;
            AnswerString = answerString;
            this.Enabled = enabled;
            if (enabled)
            {
                Status = Status | QuestionStatus.Enabled;
            }
            Instructions = instructions;
            Comments = comments;
            if(valid)
            {
                Status = Status | QuestionStatus.Valid;
            }
            var answered = !string.IsNullOrEmpty(answerString);
            if (answered)
                Status = Status | QuestionStatus.Answered;
        //    Valid = valid;
            Mandatory = mandatory;
        }

        public ItemPublicKey PublicKey { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public bool Enabled { get; private set; }
    //    public bool Valid { get; private set; }
        public string Instructions { get; private set; }

        public string Comments
        {
            get; private set; /*  get { return comments; }
              set
              {
                  comments = value;
                  RaisePropertyChanged("Comments");
              }*/ }

       // private string comments;
        public string AnswerString { get; protected set; }
        public bool Mandatory { get; private set; }
        public QuestionStatus Status { get; protected set; }


        public virtual void SetAnswer(List<Guid> answer, string answerString)
        {
            this.AnswerString = answerString;
            if (!Status.HasFlag(QuestionStatus.Answered))
            {
                Status = Status | QuestionStatus.Answered;
                RaisePropertyChanged("Status");
            }
            RaisePropertyChanged("AnswerString");
        }
        public virtual void SetComment(string comment)
        {
            this.Comments = comment;
            RaisePropertyChanged("Comments");
        }

        /* public IMvxLanguageBinder BorderColor
        {
            get { return new BorderColorConverter(Constants.GeneralNamespace, GetType().Name); }
        }*/
    }
    [Flags]
    public enum QuestionStatus
    {
        None=0,
        Enabled=1,
        Valid=2,
        Answered=4
    }
}