using System;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails.GridItems
{
    public class HeaderItem
    {
        public HeaderItem(Guid publicKey, string title, string instructions)
        {
            PublicKey = publicKey;
            Title = title;
            Instructions = instructions;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public string Instructions { get; private set; }
    }
}