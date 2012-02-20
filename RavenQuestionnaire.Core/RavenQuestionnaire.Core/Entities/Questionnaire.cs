using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;

namespace RavenQuestionnaire.Core.Entities
{
    public class Questionnaire : IEntity<QuestionnaireDocument>, IComposite
    {
        private QuestionnaireDocument innerDocument;

        public string QuestionnaireId { get { return innerDocument.Id; } }

        public Questionnaire(string title)
        {
            innerDocument = new QuestionnaireDocument() {Title = title};
        }
        public Questionnaire(QuestionnaireDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }
        public void UpdateText(string text)
        {
            innerDocument.Title = text;
            innerDocument.LastEntryDate = DateTime.Now;
        }
        public void ClearQuestions()
        {
            innerDocument.Questions.Clear();
        }

        public Question AddQuestion(string text, string stataExportCaption, QuestionType type, string condition, Guid? groupPublicKey)
        {
            
            Question result = new Question() {QuestionText = text, QuestionType = type, StataExportCaption = stataExportCaption};
            result.ConditionExpression = condition;
            if(!Add(result, groupPublicKey))
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", groupPublicKey.Value));
            return result;
        }
        public void AddGroup(string groupText,bool propageted, Guid? parent)
        {
            Group group = new Group();
            group.GroupText = groupText;
            group.Propagated = propageted;
            if (!Add(group, parent))
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
        }

        public void UpdateGroup(string groupText, bool propageted, Guid publicKey)
        {
            Group group = Find<Group>(publicKey);
            if (group != null)
            {
                group.Propagated = propageted;
                group.Update(groupText);
                return;
            }
            throw new ArgumentException(string.Format("group with  publick key {0} can't be found", publicKey));
        }
        QuestionnaireDocument IEntity<QuestionnaireDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }
        public void UpdateQuestion(Guid publicKey, string text, string stataExportCaption, QuestionType type, string condition, IEnumerable<Answer> answers)
        {
            var question = new Questionnaire(this.innerDocument).Find<Question>(publicKey);
            if (question == null)
                return;
            question.QuestionText = text;
            question.StataExportCaption = stataExportCaption;
            question.QuestionType = type;
            question.UpdateAnswerList(answers);
            question.ConditionExpression = condition;
        }

        public void UpdateConditionExpression(Guid publicKey, string condition)
        {
            var question = new Questionnaire(this.innerDocument).Find<Question>(publicKey);
            if (question == null)
                return;
            question.ConditionExpression = condition;
        }

        public bool Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue)
            {
                Group group = c as Group;
                if (group != null)
                {
                    innerDocument.Groups.Add(group);
                    return true;
                }
                Question question = c as Question;
                if (question != null)
                {
                    innerDocument.Questions.Add(question);
                    return true;
                }
            }
            foreach (Group child in innerDocument.Groups)
            {
                if (child.Add(c, parent))
                    return true;
            }
            foreach (Question child in innerDocument.Questions)
            {
                if (child.Add(c, parent))
                    return true;
            }
            return false;
        }

        public bool Remove(IComposite c)
        {
            foreach (Group child in innerDocument.Groups)
            {
                if (child == c)
                {
                    innerDocument.Groups.Remove(child);
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            foreach (Question child in innerDocument.Questions)
            {
                if (child == c)
                {
                    innerDocument.Questions.Remove(child);
                    return true;
                }
                if (child.Remove(c))
                    return true;
            }
            return false;
        }
        public bool Remove<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (Group child in innerDocument.Groups)
            {
                if (child.PublicKey == publicKey)
                {
                    innerDocument.Groups.Remove(child);
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            foreach (Question child in innerDocument.Questions)
            {
                if (child.PublicKey == publicKey)
                {
                    innerDocument.Questions.Remove(child);
                    return true;
                }
                if (child.Remove<T>(publicKey))
                    return true;
            }
            return false;
        }
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            foreach (Group child in innerDocument.Groups)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            foreach (Question child in innerDocument.Questions)
            {
                if (child is T && child.PublicKey == publicKey)
                    return child as T;
                T subNodes = child.Find<T>(publicKey);
                if (subNodes != null)
                    return subNodes;
            }
            return null;
        }

        public IList<Question> GetAllQuestions()
        {
            List<Question> result = new List<Question>();
            result.AddRange(innerDocument.Questions);
            Queue<Group> groups = new Queue<Group>();
            foreach (var child in innerDocument.Groups)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue();
                result.AddRange(queueItem.Questions);
                foreach (var child in queueItem.Groups)
                {
                    groups.Enqueue(child);
                }
            }
            return result;
        }


    }
}
