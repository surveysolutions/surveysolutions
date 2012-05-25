using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class CompleteGroupIterator : Iterator<ICompleteGroup>
    {
        private int current;
        protected IList<ICompleteGroup> groups;

        #region Implementation of IEnumerable

        public IEnumerator<ICompleteGroup> GetEnumerator()
        {
            return groups.GetEnumerator();
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
            if (current < groups.Count - 1)
            {
                current++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            current = 0;
        }

        public ICompleteGroup Current
        {
            get { return groups[current]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion

        public CompleteGroupIterator(ICompleteGroup document)
        {
            if (document.Children.Count == 0)
                throw new ArgumentException("Questionnaires question list is empty");

            groups = new List<ICompleteGroup>();

            IList<ICompleteGroup> allGroups = new List<ICompleteGroup>();

            var queue = new Queue<ICompleteGroup>();
            queue.Enqueue(document);
            while (queue.Count != 0)
            {
                ICompleteGroup item = queue.Dequeue();
                List<IComposite> innerGroups = item.Children.Where(c => c is ICompleteGroup).ToList();
                ICompleteGroup prevGroup = null;
                foreach (CompleteGroup @group in innerGroups)
                {
                    @group.PrevGroup = prevGroup;
                    if (prevGroup != null)
                        prevGroup.NextGroup = @group;
                    @group.ParentGroup = item;
                    queue.Enqueue(@group);
                    prevGroup = @group;
                }
                allGroups.Add(item);
            }

            groups = allGroups;
        }

        #region Iterator<ICompleteGroup> Members

        public ICompleteGroup Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return Current;
            }
        }

        public ICompleteGroup Previous
        {
            get
            {
                if (current < 1)
                    return null;
                return groups[--current];
            }
        }

        public void SetCurrent(ICompleteGroup item)
        {
            int index = groups.IndexOf(item);
            if (index >= 0)
                current = index;
            else
            {
                throw new ArgumentOutOfRangeException("groups is absent");
            }
        }

        #endregion
    }
}