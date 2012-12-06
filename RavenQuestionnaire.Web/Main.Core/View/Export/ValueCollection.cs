// -----------------------------------------------------------------------
// <copyright file="ValueCollection.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

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
    public class ValueCollection : IEnumerable<KeyValuePair<Guid, IEnumerable<string>>>
    {
        protected readonly IDictionary<Guid,IEnumerable<string>> container;

        public ValueCollection()
        {
            this.container = new Dictionary<Guid, IEnumerable<string>>();
        }

        public IEnumerable<string> this[Guid publickey]
        {
            get
            {
                if (this.container.ContainsKey(publickey)) return this.container[publickey];
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
                this.container.Add(key, new string[] {""});
                return;
            }

           

            if (question.QuestionType == QuestionType.MultyOption)
            {
                var answers = new List<string>();
                foreach (ICompleteAnswer answer in question.Answers)
                {
                    answers.Add(answer.Selected ? "1" : "0");
                }
                this.container.Add(key, answers);
                /*  var answers = answer as object[];
                if (answers != null)
                    this.container.Add(key, answers.Select(s => s.ToString()));
                else
                    this.container.Add(key, new string[] {""});*/
            }
            else
            {

                var answer = question.GetAnswerObject();
                this.container.Add(key, new string[] {answer == null ? "" : answer.ToString()});
            }
        }

        #region Implementation of IEnumerable

        public IEnumerator<KeyValuePair<Guid, IEnumerable<string>>> GetEnumerator()
        {
            return this.container.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
