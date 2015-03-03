using System;

namespace WB.UI.Capi.ViewModel.Dashboard
{
    public class FeaturedItem
    {
        public FeaturedItem(Guid publicKey, string title, string value)
        {
            PublicKey = publicKey;
            Title = title;
            Value = value;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }

        public string Value { get; set; }
    }
}
