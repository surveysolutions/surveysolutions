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
    public class QuestionnaireAnswerIterator : Iterator<ICompleteAnswer>
    {
        public QuestionnaireAnswerIterator(ICompleteGroup<ICompleteGroup,ICompleteQuestion> document)
        {
            answers = new List<ICompleteAnswer>();
            foreach (ICompleteQuestion completeQuestion in document.Questions)
            {
                var questionWithAnswers = completeQuestion as ICompleteQuestion<ICompleteAnswer>;
                if (questionWithAnswers == null)
                    continue;

                foreach (CompleteAnswer completeAnswer in questionWithAnswers.Answers)
                {
                    completeAnswer.QuestionPublicKey = completeQuestion.PublicKey;
                    if (completeAnswer.Selected)
                        answers.Add(completeAnswer);
                }
            }
            Queue<ICompleteGroup> groups = new Queue<ICompleteGroup>();
            foreach (var child in document.Groups)
            {
                var childWithGroups = child as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
                if(childWithGroups==null)
                    continue;
                
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue() as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
               
                if (queueItem == null)
                    continue;
                foreach (ICompleteQuestion completeQuestion in queueItem.Questions)
                {
                    var questionWithAnswers = completeQuestion as ICompleteQuestion<ICompleteAnswer>;
                    if (questionWithAnswers == null)
                        continue;
                    foreach (CompleteAnswer completeAnswer in questionWithAnswers.Answers)
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

        protected List<ICompleteAnswer> answers;
        private int current = 0;


        #region Implementation of Iterator<CompleteAnswer>

        public ICompleteAnswer Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return this.answers[++this.current];
            }
        }

        public ICompleteAnswer Previous
        {
            get
            {
                if (this.current < 1)
                    return null;
                return this.answers[--this.current];
            }
        }
        public void SetCurrent(ICompleteAnswer item)
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

        public IEnumerator<ICompleteAnswer> GetEnumerator()
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

        public ICompleteAnswer Current
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
