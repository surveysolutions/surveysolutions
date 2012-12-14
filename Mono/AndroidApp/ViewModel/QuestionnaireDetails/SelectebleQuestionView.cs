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
    public class SelectebleQuestionView:QuestionView
    {
        public SelectebleQuestionView(Guid publicKey, string text, QuestionType type, IEnumerable<AnswerView> answers, bool enabled, string instructions)
            : base(publicKey, text, type,enabled,instructions)
        {
            Answers = answers;
        }

        public IEnumerable<AnswerView> Answers { get; private set; }
    }
}