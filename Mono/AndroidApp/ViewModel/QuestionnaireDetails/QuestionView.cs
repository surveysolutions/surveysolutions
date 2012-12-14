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
        public QuestionView(Guid publicKey, string text, QuestionType type, bool enabled, string instructions,string comments, bool valid, bool mandatory)
        {
            PublicKey = publicKey;
            Text = text;
            QuestionType = type;
            Enabled = enabled;
            Instructions = instructions;
            Comments = comments;
            Valid = valid;
            Mandatory = mandatory;
        }

        public Guid PublicKey { get; private set; }
        public string Text { get; private set; }
        public QuestionType QuestionType { get; private set; }
        public bool Enabled { get; private set; }
        public bool Valid { get; private set; }
        public string Instructions { get; private set; }
        public string Comments { get; private set; }
        public bool Answered { get; protected set; }
        public bool Mandatory { get; private set; }
    }
}