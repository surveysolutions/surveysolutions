using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
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

        public void AddGroup(string groupText,Propagate propageted, Guid? parent)
        {
            Group group = new Group();
            group.Title = groupText;
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

        public void UpdateGroup(string groupText, Propagate propageted, Guid publicKey)
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
            var question = Find<Question>(publicKey);
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
            var question = Find<Question>(publicKey);
            if (question == null)
                return;
            question.ConditionExpression = condition;
        }
        public void Add(IComposite c, Guid? parent)
        {
            innerDocument.Add(c, parent);
        }

        public void Remove(IComposite c)
        {
           innerDocument.Remove(c);
        }
        public void Remove<T>(Guid publicKey) where T : class, IComposite
        {
            innerDocument.Remove<T>(publicKey);
        }
        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return innerDocument.Find<T>(publicKey);
        }
        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class, IComposite
        {
            return
                innerDocument.Find<T>(condition);
        }

        public IList<IQuestion> GetAllQuestions()
        {
            List<IQuestion> result = new List<IQuestion>();
            result.AddRange(innerDocument.Questions);
            Queue<IGroup> groups = new Queue<IGroup>();
            foreach (var child in innerDocument.Groups)
            {
                groups.Enqueue(child);
            }
            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue() as IGroup<IGroup, IQuestion>;
                if (queueItem == null)
                    continue;
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
