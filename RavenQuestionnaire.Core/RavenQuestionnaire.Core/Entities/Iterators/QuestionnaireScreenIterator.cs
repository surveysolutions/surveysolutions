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
    public class QuestionnaireScreenIterator : Iterator<CompleteGroup>
    {
        public QuestionnaireScreenIterator(IQuestionnaireDocument<CompleteGroup, CompleteQuestion> questionnaire)
        {
            if (questionnaire.Questions.Count == 0)
            {
                if (questionnaire.Groups.Count == 0)
                    throw new ArgumentException("Questionnaires question list is empty");
                this.groups = questionnaire.Groups;
            }
            else
            {
                this.groups = new List<CompleteGroup>(questionnaire.Groups.Count + 1);
                this.groups.Add(new CompleteGroup() { Questions = questionnaire.Questions, PublicKey = Guid.Empty });
                foreach (CompleteGroup item in questionnaire.Groups)
                {
                    this.groups.Add(item);
                }
            }
        }
        protected IList<CompleteGroup> groups;
        public CompleteGroup Next
        {
            get
            {
                if (!MoveNext())
                    return null;
                return this.Current;
            }
        }

        public CompleteGroup Previous
        {
            get
            {
                if (this.current < 1)
                    return null;
                return this.groups[--this.current];
            }
        }
        public void SetCurrent(CompleteGroup item)
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

        public IEnumerator<CompleteGroup> GetEnumerator()
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
            if(this.current < this.groups.Count-1)
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

        public CompleteGroup Current
        {
            get { return this.groups[current];}
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}
