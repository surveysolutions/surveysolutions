using System;

namespace WB.Core.BoundedContexts.Interviewer.ViewModel.Dashboard
{
    public class FeaturedItem
    {
        public FeaturedItem(Guid publicKey, string title, string value, bool statsInvisible)
        {
            this.PublicKey = publicKey;
            this.Title = title;
            this.Value = value;
            this.StatsInvisible = statsInvisible;
        }

        public Guid PublicKey { get; private set; }
        public string Title { get; private set; }

        public string Value { get; set; }

        public bool StatsInvisible { get; set; }
    }
}
