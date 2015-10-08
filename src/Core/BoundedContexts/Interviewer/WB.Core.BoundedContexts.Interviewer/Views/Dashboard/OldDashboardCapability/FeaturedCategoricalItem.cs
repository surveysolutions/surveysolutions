using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard
{
    public class FeaturedCategoricalItem : FeaturedItem
    {
        public FeaturedCategoricalItem(Guid publicKey, string title, string value,
            IEnumerable<FeaturedCategoricalOption> options, bool statsInvisible) : base(publicKey: publicKey, title: title, value: value, statsInvisible: statsInvisible)
        {
            this.Options = options;
        }

        public IEnumerable<FeaturedCategoricalOption> Options { get; private set; }
    }
}
