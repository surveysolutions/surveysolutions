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
    public abstract class QuestionView
    {
        public QuestionView(Guid publicKey, string text, QuestionType type, bool enabled, string instructions)
        {
            PublicKey = publicKey;
            Text = text;
            QuestionType = type;
            Enabled = enabled;
            Instructions = instructions;
        }

        public Guid PublicKey { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public bool Enabled { get; private set; }
        public string Instructions { get; private set; }
    }
}