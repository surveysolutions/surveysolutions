using System;

namespace WB.Core.BoundedContexts.CAPI.Views.InterviewDetails.GridItems
{
    public class HeaderItem
    {
        public HeaderItem(Guid publicKey, string title, string instructions)
        {
            this.PublicKey = publicKey;
            this.Title = title;
            this.Instructions = instructions;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }
        public string Instructions { get; private set; }
    }
}