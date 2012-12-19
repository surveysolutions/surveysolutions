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

namespace AndroidApp.ViewModel.QuestionnaireDetails
{
    public class SelectebleQuestionViewModel:QuestionViewModel
    {
        public SelectebleQuestionViewModel(ItemPublicKey publicKey, string text, QuestionType type, IEnumerable<AnswerViewModel> answers, bool enabled, string instructions, string comments, bool valid, bool mandatory)
            : base(publicKey, text, type,enabled,instructions,comments,valid, mandatory)
        {
            Answers = answers;
            Answered = answers.Any(a => a.Selected);
        }

        public IEnumerable<AnswerViewModel> Answers { get; private set; }
    }
}