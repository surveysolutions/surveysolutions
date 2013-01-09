using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails.GridItems;
using Main.Core.Entities.SubEntities;
using Cirrious.MvvmCross.ViewModels;
namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class SelectebleQuestionViewModel:QuestionViewModel
    {
        public SelectebleQuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, IEnumerable<AnswerViewModel> answers, bool enabled, string instructions, string comments, bool valid, bool mandatory)
            : base(publicKey, text, type,enabled,instructions,comments,valid, mandatory)
        {
            Answers = answers;
            var answered = answers.Any(a => a.Selected);
            if(answered)
                Status = Status | QuestionStatus.Answered;
            //Answered = answers.Any(a => a.Selected);
        }

        public SelectebleQuestionViewModel(AbstractQuestionRowItem rosterItem, HeaderItem headerItem) : base(rosterItem, headerItem)
        {
            var typedRoaster = rosterItem as SelectableRowItem;
            if (typedRoaster == null)
                throw new ArgumentException();
            this.Answers = typedRoaster.Answers;
        }

        public IEnumerable<AnswerViewModel> Answers { get; private set; }
        public void SelectAnswer(Guid publicKey)
        {
            foreach (var answer in Answers)
            {
                answer.Selected = answer.PublicKey == publicKey;
            }
            if (!Status.HasFlag(QuestionStatus.Answered))
            {
                Status = Status | QuestionStatus.Answered;
                RaisePropertyChanged("Status");
            }
        }
    }
}