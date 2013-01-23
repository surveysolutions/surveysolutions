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
        public SelectebleQuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, IEnumerable<AnswerViewModel> answers, bool enabled, string instructions, string comments, bool valid, bool mandatory, string answerString)
            : base(publicKey, text, type, enabled, instructions, comments, valid, mandatory, answerString)
        {
            Answers = answers;
          
            //Answered = answers.Any(a => a.Selected);
        }

     /*   public SelectebleQuestionViewModel(AbstractQuestionRowItem rosterItem, HeaderItem headerItem) : base(rosterItem, headerItem)
        {
            var typedRoaster = rosterItem as SelectableRowItem;
            if (typedRoaster == null)
                throw new ArgumentException();
            this.Answers = typedRoaster.Answers;
        }
        */
        public IEnumerable<AnswerViewModel> Answers { get; private set; }
        public override void SetAnswer(List<Guid> answer, string answerString)
        {
            if (answer == null)
            {
                return;
            }
            foreach (var item in this.Answers)
            {
                item.Selected = answer.Contains(item.PublicKey);
            }
            base.SetAnswer(answer, answerString);
        }

        /*public void SelectAnswer(Guid publicKey)
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
            RaisePropertyChanged("AnswerString");
        }*/
    }
}