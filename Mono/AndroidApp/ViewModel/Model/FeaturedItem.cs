using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndroidApp.ViewModel.Model
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
        public string Value { get; private set; }
    }
}
