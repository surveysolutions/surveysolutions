using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireScreenIterator : Iterator<ICompleteGroup>
    {
        public QuestionnaireScreenIterator(IQuestionnaireDocument document)
        {
            var innerGroups = document.Children.Where(c => c is ICompleteGroup).ToList();
            var innerQuestions = document.Children.Where(c => c is ICompleteQuestion).ToList();

            if (document.Children.Count == 0)

                throw new ArgumentException("Questionnaires question list is empty");


            this.groups = new List<ICompleteGroup>(innerGroups.Count + 1);
            if (innerQuestions.Count > 0)
                this.groups.Add(new CompleteGroup() { Children = innerQuestions, PublicKey = Guid.Empty });
            foreach (CompleteGroup item in innerGroups)
            {
                this.groups.Add(item);
            }

        }

        protected IList<ICompleteGroup> groups;
        public ICompleteGroup Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return this.Current;
            }
        }

        public ICompleteGroup Previous
        {
            get
            {
                if (this.current < 1)
                    return null;
                return this.groups[--this.current];
            }
        }
        public void SetCurrent(ICompleteGroup item)
        {
            var index = this.groups.IndexOf(item);
            if (index >= 0)
                this.current = index;
            else
            {
                throw new ArgumentOutOfRangeException("groups is absent");
            }
        }
        private int current = 0;

        #region Implementation of IEnumerable

        public IEnumerator<ICompleteGroup> GetEnumerator()
        {
            return this.groups.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            if (this.current < this.groups.Count - 1)
            {
                this.current++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            this.current = 0;
        }

        public ICompleteGroup Current
        {
            get { return this.groups[current]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}
