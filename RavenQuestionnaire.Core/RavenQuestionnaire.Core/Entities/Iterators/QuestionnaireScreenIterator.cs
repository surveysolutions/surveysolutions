using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireScreenIterator : Iterator<CompleteGroup, Guid>
    {
        public QuestionnaireScreenIterator(CompleteQuestionnaire questionnaire)
        {
            this.questionnaire = questionnaire;
           
            if (this.questionnaire.GetRootQuestions().Count == 0)
            {
                if (this.questionnaire.GetAllGroups().Count == 0)
                    throw new ArgumentException("Questionnaires question list is empty");
                this.groups = this.questionnaire.GetAllGroups();
            }
            else
            {
                this.groups = new List<CompleteGroup>(this.questionnaire.GetAllGroups().Count + 1);
                this.groups.Add(new CompleteGroup() { Questions = this.questionnaire.GetRootQuestions().ToList(), PublicKey = Guid.Empty });
                foreach (CompleteGroup item in this.questionnaire.GetAllGroups())
                {
                    this.groups.Add(item);
                }
            }
        }
        protected CompleteQuestionnaire questionnaire;
        protected IList<CompleteGroup> groups;
        public CompleteGroup First
        {
            get { return groups.First(); }
        }

        public CompleteGroup Last
        {
            get { return this.groups[this.groups.Count - 1]; }
        }

        public CompleteGroup Next
        {
            get
            {
                if (IsDone)
                    return null;
                return this.groups[++this.current];
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

        public bool IsDone
        {
            get { return this.current >= this.groups.Count - 1; }
        }

        public CompleteGroup CurrentItem
        {
            get { return this.groups[current]; }
        }

        public CompleteGroup GetNextAfter(Guid key)
        {
            var group =
                this.groups.FirstOrDefault(q => q.PublicKey.Equals(key));

            if (group != null)
            {
                current = this.groups.IndexOf(group);
                return Next;
            }
            return null;
        }

        public CompleteGroup GetPreviousBefoure(Guid key)
        {
            var group =
                this.groups.FirstOrDefault(q => q.PublicKey.Equals(key));
           
            if (group != null)
            {
                current = this.groups.IndexOf(group);
                return Previous;
            }
            return null;
        }
        private int current = 0;
    }
}
