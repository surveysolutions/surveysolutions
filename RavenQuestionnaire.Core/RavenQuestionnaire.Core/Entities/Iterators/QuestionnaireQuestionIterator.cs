using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireQuestionIterator : Iterator<ICompleteQuestion>
    {
        public QuestionnaireQuestionIterator(ICompleteQuestionnaireDocument<ICompleteGroup, ICompleteQuestion> document)
        {
          //  this.questionnaire = questionnaire;
            questions = new List<ICompleteQuestion>();
            questions.AddRange(document.Questions);
            Queue<ICompleteGroup> groups = new Queue<ICompleteGroup>();
            foreach (var child in document.Groups)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue() as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
                if (queueItem == null)
                    continue;
                questions.AddRange(queueItem.Questions);
                foreach (var child in queueItem.Groups)
                {
                    groups.Enqueue(child);
                }
            }
        }
    //    protected IQuestionnaireDocument<CompleteGroup, ICompleteQuestion> questionnaire;
        protected List<ICompleteQuestion> questions;
        private int current = 0;
        #region Implementation of Iterator<Question,Guid>

        public ICompleteQuestion Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return this.questions[++this.current];
            }
        }

        public ICompleteQuestion Previous
        {
            get
            {
                if (this.current < 1)
                    return null;
                return this.questions[--this.current];
            }
        }
        public void SetCurrent(ICompleteQuestion item)
        {
            var index = this.questions.IndexOf(item);
            if (index >= 0)
                this.current = index;
            else
            {
                throw new ArgumentOutOfRangeException("question is absent");
            }
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<ICompleteQuestion> GetEnumerator()
        {
            return this.questions.GetEnumerator();
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
            if (this.current < this.questions.Count-1)
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

        public ICompleteQuestion Current
        {
            get { return this.questions[current]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}
