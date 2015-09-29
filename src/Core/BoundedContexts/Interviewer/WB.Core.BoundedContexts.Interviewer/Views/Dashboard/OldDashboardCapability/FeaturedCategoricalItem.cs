using System;
using System.Collections.Generic;

namespace WB.UI.Capi.ViewModel.Dashboard
{
    public class FeaturedCategoricalItem : FeaturedItem
    {
        public FeaturedCategoricalItem(Guid publicKey, string title, string value,
            IEnumerable<FeaturedCategoricalOption> options) : base(publicKey: publicKey, title: title, value: value)
        {
            this.Options = options;
        }

        public IEnumerable<FeaturedCategoricalOption> Options { get; private set; }
    }
}
