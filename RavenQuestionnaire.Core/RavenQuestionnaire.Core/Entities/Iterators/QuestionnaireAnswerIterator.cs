using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireAnswerIterator : Iterator<CompleteAnswer>
    {
        public QuestionnaireAnswerIterator(IQuestionnaireDocument<CompleteGroup, CompleteQuestion> document)
        {
            answers = new List<CompleteAnswer>();
            foreach (CompleteQuestion completeQuestion in document.Questions)
            {
                foreach (CompleteAnswer completeAnswer in completeQuestion.Answers)
                {
                    completeAnswer.QuestionPublicKey = completeQuestion.PublicKey;
                    if (completeAnswer.Selected)
                        answers.Add(completeAnswer);
                }
            }
            Queue<CompleteGroup> groups = new Queue<CompleteGroup>();
            foreach (var child in document.Groups)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue();
                foreach (CompleteQuestion completeQuestion in queueItem.Questions)
                {
                    foreach (CompleteAnswer completeAnswer in completeQuestion.Answers)
                    {
                        completeAnswer.QuestionPublicKey = completeQuestion.PublicKey;
                        if (completeAnswer.Selected)
                            answers.Add(completeAnswer);
                    }
                }
                foreach (var child in queueItem.Groups)
                {
                    groups.Enqueue(child);
                }
            }
        }

        protected List<CompleteAnswer> answers;
        private int current = 0;


        #region Implementation of Iterator<CompleteAnswer>

        public CompleteAnswer Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return this.answers[++this.current];
            }
        }

        public CompleteAnswer Previous
        {
            get
            {
                if (this.current < 1)
                    return null;
                return this.answers[--this.current];
            }
        }
        public void SetCurrent(CompleteAnswer item)
        {
            var index = this.answers.IndexOf(item);
            if (index >= 0)
                this.current = index;
            else
            {
                throw new ArgumentOutOfRangeException("answer is absent");
            }
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<CompleteAnswer> GetEnumerator()
        {
            return this.answers.GetEnumerator();
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
            if (this.current < this.answers.Count-1)
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

        public CompleteAnswer Current
        {
            get { return this.answers[current]; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}
