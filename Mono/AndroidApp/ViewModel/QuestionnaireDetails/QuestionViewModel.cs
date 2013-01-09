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
using Main.Core.Entities.SubEntities;

namespace AndroidApp.ViewModel.QuestionnaireDetails
{


    public abstract class QuestionViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireItemViewModel
    {
        public QuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, bool enabled, string instructions, string comments, bool valid, bool mandatory)
        {
            PublicKey = publicKey;
            Text = text;
            QuestionType = type;
            this.Enabled = enabled;
            this.Status = QuestionStatus.None;
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
            get { return comments; }
            set
            {
                comments = value;
                RaisePropertyChanged("Comments");
            }
        }

        private string comments;
      //  public bool Answered { get; protected set; }
        public bool Mandatory { get; private set; }
        public QuestionStatus Status { get; protected set; }

        

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