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
    public class QuestionnaireQuestionIterator : Iterator<CompleteQuestion>
    {
        public QuestionnaireQuestionIterator(IQuestionnaireDocument<CompleteGroup, CompleteQuestion> questionnaire)
        {
          //  this.questionnaire = questionnaire;
            questions = new List<CompleteQuestion>();
            questions.AddRange(questionnaire.Questions);
            Queue<CompleteGroup> groups = new Queue<CompleteGroup>();
            foreach (var child in questionnaire.Groups)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue();
                questions.AddRange(queueItem.Questions);
                foreach (var child in queueItem.Groups)
                {
                    groups.Enqueue(child);
                }
            }
        }
    //    protected IQuestionnaireDocument<CompleteGroup, CompleteQuestion> questionnaire;
        protected List<CompleteQuestion> questions;
        private int current = 0;
        #region Implementation of Iterator<Question,Guid>

        public CompleteQuestion Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return this.questions[++this.current];
            }
        }

        public CompleteQuestion Previous
        {
            get
            {
                if (this.current < 1)
                    return null;
                return this.questions[--this.current];
            }
        }
        public void SetCurrent(CompleteQuestion item)
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

        public IEnumerator<CompleteQuestion> GetEnumerator()
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

        public CompleteQuestion Current
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
