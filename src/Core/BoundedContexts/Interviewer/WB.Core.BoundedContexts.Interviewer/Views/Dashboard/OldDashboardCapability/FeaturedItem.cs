using System;

namespace WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard
{
    [Obsolete]
    public class FeaturedItem
    {
        public FeaturedItem(Guid publicKey, string title, string value)
        {
            this.PublicKey = publicKey;
            this.Title = title;
            this.Value = value;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }

        public string Value { get; set; }
        
    }
}
