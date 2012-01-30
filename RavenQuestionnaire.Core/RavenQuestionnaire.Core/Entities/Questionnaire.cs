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

            Question result = new Question()
                                  {QuestionText = text, QuestionType = type, StataExportCaption = stataExportCaption};
            result.ConditionExpression = condition;
            try
            {
                Add(result, groupPublicKey);
                return result;
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found",
                                                          groupPublicKey.Value));
            }
        }

        public void AddGroup(string groupText,bool propageted, Guid? parent)
        {
            Group group = new Group();
            group.GroupText = groupText;
            group.Propagated = propageted;
            try
            {
                Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
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
        public void UpdateFlow(List<FlowBlock> blocks, List<FlowConnection> connections)
        {
            //TODO: check flow saving!
            if (this.innerDocument.FlowGraph==null)
            {
                this.innerDocument.FlowGraph = new FlowGraph();
            }
            var graph = this.innerDocument.FlowGraph;
            graph.Blocks = blocks;
            graph.Connections = connections;
        }
        public void Add(IComposite c, Guid? parent)
        {
            if (!parent.HasValue)
            {
                Group group = c as Group;
                if (group != null)
                {
                    innerDocument.Groups.Add(group);
                    return;
                }
                Question question = c as Question;
                if (question != null)
                {
                    innerDocument.Questions.Add(question);
                    return;
                }
            }
            foreach (Group child in innerDocument.Groups)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
                /* if (child.Add(c, parent))
                     return true;*/
            }
            foreach (Question child in innerDocument.Questions)
            {
                try
                {
                    child.Add(c, parent);
                    return;
                }
                catch (CompositeException)
                {
                }
                /*  if (child.Add(c, parent))
                      return true;*/
            }
            throw new CompositeException();
        }

        public void Remove(IComposite c)
        {
            var group = this.innerDocument.Groups.FirstOrDefault(g =>c is Group &&  g.PublicKey.Equals(((Group)c).PublicKey));
            if (group != null)
            {
                this.innerDocument.Groups.Remove(group);
                return;
            }
            var question = this.innerDocument.Questions.FirstOrDefault(g => c is Question && g.PublicKey.Equals(((Question)c).PublicKey));
            if (question != null)
            {
                this.innerDocument.Questions.Remove(question);
                return;
            }
            foreach (Group child in this.innerDocument.Groups)
            {
                try
                {
                    child.Remove(c);
                    return;
                }
                catch (CompositeException)
                {

                }
            }
            foreach (Question child in this.innerDocument.Questions)
            {
                try
                {
                    child.Remove(c);
                    return;
                }
                catch (CompositeException)
                {

                }
            }
            throw new CompositeException();
        }
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            var group = this.innerDocument.Groups.FirstOrDefault(g => typeof(T) == typeof(Group) && g.PublicKey.Equals(publicKey));
            if (group != null)
            {
                this.innerDocument.Groups.Remove(group);
                return;
            }
            var question = this.innerDocument.Questions.FirstOrDefault(g => typeof(T) == typeof(Question) && g.PublicKey.Equals(publicKey));
            if (question != null)
            {
                this.innerDocument.Questions.Remove(question);
                return;
            }
            foreach (Group child in this.innerDocument.Groups)
            {
                try
                {
                    child.Remove<T>(publicKey);
                    return;
                }
                catch (CompositeException)
                {

                }
            }
            foreach (Question child in this.innerDocument.Questions)
            {
                try
                {
                    child.Remove<T>(publicKey);
                    return;
                }
                catch (CompositeException)
                {

                }
            }
            throw new CompositeException();
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
