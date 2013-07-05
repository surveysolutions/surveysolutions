namespace Main.Core.View.Export
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ValueCollection : IEnumerable<KeyValuePair<Guid, IEnumerable<string>>>
    {
        protected readonly IDictionary<Guid,IEnumerable<string>> container;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueCollection"/> class.
        /// </summary>
        public ValueCollection()
        {
            this.container = new Dictionary<Guid, IEnumerable<string>>();
        }

        public IEnumerable<string> this[Guid publickey]
        {
            get
            {
                if (this.container.ContainsKey(publickey))
                {
                    return this.container[publickey];
                }

                return Enumerable.Empty<string>();
            }
        }

        public void AddRange(ValueCollection collection)
        {
            foreach (KeyValuePair<Guid, IEnumerable<string>> keyValuePair in collection)
            {
                this.container.Add(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public void Add(Guid key, ICompleteQuestion question)
        {
            if (question == null)
            {
                this.container.Add(key, new string[] {string.Empty});
                return;
            }

            if (question is IMultyOptionsQuestion)
            {
                var answers = new List<string>();
                foreach (ICompleteAnswer answer in question.Answers)
                {
                    answers.Add(!question.Enabled ? string.Empty : answer.Selected ? "1" : "0");
                }
                
                this.container.Add(key, answers);
            }

            else
            {
                if (!question.Enabled)
                {
                    this.container.Add(key, new string[] {string.Empty});
                    return;
                }
                var answer = question.GetAnswerObject();
                this.container.Add(key, new[] { answer == null ? "" : answer.ToString() });
            }
        }

        #region Implementation of IEnumerable

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator<KeyValuePair<Guid, IEnumerable<string>>> GetEnumerator()
        {
            return this.container.GetEnumerator();
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
