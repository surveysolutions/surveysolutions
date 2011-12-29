using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    public class QuestionnaireScreenIterator : Iterator<Group, Guid?>
    {
        public QuestionnaireScreenIterator(CompleteQuestionnaire questionnaire)
        {
            this.questionnaire = questionnaire;
           
            if (this.questionnaire.GetAllQuestions().Count == 0)
            {
                if (this.questionnaire.GetAllGroups().Count == 0)
                    throw new ArgumentException("Questionnaires question list is empty");
                this.groups = this.questionnaire.GetAllGroups();
            }
            else
            {
                this.groups = new List<Group>(this.questionnaire.GetAllGroups().Count + 1);
                this.groups.Add(new Group()
                               {Questions = this.questionnaire.GetAllQuestions().ToList(), PublicKey = Guid.Empty});
                foreach (Group item in this.questionnaire.GetAllGroups())
                {
                    this.groups.Add(item);
                }
            }
        }
        protected CompleteQuestionnaire questionnaire;
        protected IList<Group> groups; 
        public Group First
        {
            get { return groups.First(); }
        }

        public Group Last
        {
            get { return this.groups[this.groups.Count - 1]; }
        }

        public Group Next
        {
            get
            {
                if (IsDone)
                    return null;
                return this.groups[++this.current];
            }
        }

        public Group Previous
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

        public Group CurrentItem
        {
            get { return this.groups[current]; }
        }

        public Group GetNextAfter(Guid? key)
        {
            if (!key.HasValue)
                return First;
            var group =
                this.groups.Where(q => q.PublicKey.Equals(key.Value)).FirstOrDefault();

            if (group != null)
            {
                current = this.groups.IndexOf(group);
                return Next;
            }
            return null;
        }

        public Group GetPreviousBefoure(Guid? key)
        {
            if (!key.HasValue)
                return Last;
            var group =
                this.groups.Where(q => q.PublicKey.Equals(key)).FirstOrDefault();
           
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
