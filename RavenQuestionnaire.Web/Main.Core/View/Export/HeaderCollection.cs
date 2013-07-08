using System.Collections;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;

namespace Main.Core.View.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class HeaderCollection : IEnumerable<HeaderItem>
    {
        protected IDictionary<Guid, IEnumerable<HeaderItem>> container;


        public HeaderCollection()
        {
            this.container = new Dictionary<Guid, IEnumerable<HeaderItem>>();
        }

        public HeaderCollection(HeaderCollection collection) : this()
        {
            foreach (var headerItem in collection.container)
            {
                this.container.Add(headerItem.Key, headerItem.Value);
            }
        }

        public void Merge(HeaderCollection collection)
        {
            foreach (var headerItem in collection.container)
            {
                this.container.Add(headerItem.Key, headerItem.Value);
            }
        }

        public void Add(IQuestion question)
        {
            if (!(question is IMultyOptionsQuestion))
            {
                this.container.Add(question.PublicKey, new HeaderItem[] {new HeaderItem(question)});
                return;
            }
            var headerItems = new List<HeaderItem>(question.Answers.Count);
            for (int i = 0; i < question.Answers.Count; i++)
            {
                headerItems.Add(new HeaderItem(question, i));

            }
            this.container.Add(question.PublicKey, headerItems);
        }

        public void Add(HeaderItem item)
        {
            if (!this.container.ContainsKey(item.PublicKey))
            {
                this.container.Add(item.PublicKey, new HeaderItem[] {item});
                return;
            }

            this.container[item.PublicKey] = this.container[item.PublicKey].Union(new HeaderItem[] {item});

        }

        public IEnumerable<Guid> Keys
        {
            get { return this.container.Select(c => c.Key); }
        }

        #region Implementation of IEnumerable

        public IEnumerator<HeaderItem> GetEnumerator()
        {
            return this.container.SelectMany(c => c.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

    }
}
