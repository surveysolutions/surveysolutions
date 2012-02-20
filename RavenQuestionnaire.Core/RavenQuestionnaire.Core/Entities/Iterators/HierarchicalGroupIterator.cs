using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class HierarchicalGroupIterator: Iterator<ICompleteGroup>
    {
        public HierarchicalGroupIterator(IQuestionnaireDocument<ICompleteGroup, ICompleteQuestion> document)
        {
          //  this.questionnaire = questionnaire;
            groups = new List<ICompleteGroup>();
            groups.AddRange(document.Groups);
            Queue<ICompleteGroup> groupsQuque = new Queue<ICompleteGroup>();
            foreach (var child in document.Groups)
            {
                groupsQuque.Enqueue(child);
            }
            while (groupsQuque.Count != 0)
            {
                var queueItem = groupsQuque.Dequeue() as ICompleteGroup<ICompleteGroup,ICompleteQuestion>;
                if(queueItem==null)
                    continue;
                
                groups.AddRange(queueItem.Groups);
                foreach (var child in queueItem.Groups)
                {
                    groupsQuque.Enqueue(child);
                }
            }
        }
    //    protected IQuestionnaireDocument<CompleteGroup, CompleteQuestion> questionnaire;
        protected List<ICompleteGroup> groups;
        private int current = 0;
        #region Implementation of Iterator<Question,Guid>

        public ICompleteGroup Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return this.groups[++this.current];
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
                throw new ArgumentOutOfRangeException("question is absent");
            }
        }

        #endregion

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
