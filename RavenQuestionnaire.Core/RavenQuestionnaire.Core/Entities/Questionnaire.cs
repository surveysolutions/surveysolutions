using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Entities
{
    public class Questionnaire : IEntity<QuestionnaireDocument>
    {
        #region Properties

        private QuestionnaireDocument innerDocument;

        public string QuestionnaireId { get { return innerDocument.Id; } }

        #endregion

        #region Constructor

        public Questionnaire(string title, Guid publicKey)
        {
            innerDocument = new QuestionnaireDocument()
            {
                Title = title,
                PublicKey = publicKey
            };
        }

        public Questionnaire(QuestionnaireDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        #endregion

        #region PublicMethod
        
        public void UpdateText(string text)
        {
            innerDocument.Title = text;
            innerDocument.LastEntryDate = DateTime.Now;
        }

        public void ClearQuestions()
        {
            innerDocument.Children.RemoveAll(a=>a is IQuestion);
        }

        public AbstractQuestion AddQuestion(Guid qid, string text, string stataExportCaption, QuestionType type, string condition, string validation, bool featured, bool mandatory, Order answerOrder, Guid? groupPublicKey,
            IEnumerable<Answer> answers, Guid publicKey)
        {
            var result = new CompleteQuestionFactory().Create(type);
            result.PublicKey = qid;
            result.QuestionType = type;
            result.QuestionText = text;
            result.StataExportCaption = stataExportCaption;
            result.ConditionExpression = condition;
            result.ValidationExpression = validation;
            result.AnswerOrder = answerOrder;
            result.Featured = featured;
            result.Mandatory = mandatory;
            result.PublicKey = publicKey;
            UpdateAnswerList(answers, result);
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

        protected void UpdateAnswerList( IEnumerable<Answer> answers, AbstractQuestion question)
        {
            if (answers != null && answers.Any())
            {
                question.Children.Clear();
                foreach (Answer answer in answers)
                {
                    question.Add(answer, question.PublicKey);
                }
            }
        }

        public void MoveItem(Guid itemPublicKey, Guid? groupKey, Guid? after)
        {
            var result= MoveItem(this.innerDocument, itemPublicKey, groupKey, after);
            if(!result)
                throw new ArgumentException(string.Format("item doesn't exists -{0}", itemPublicKey));
        }

        protected bool MoveItem(IComposite root, Guid itemPublicKey, Guid? groupKey,  Guid? after)
        {
            if (Move(root.Children, itemPublicKey, groupKey, after))
                return true;

            foreach (IComposite group in root.Children)
            {
                if (MoveItem(group, itemPublicKey, groupKey, after))
                    return true;
            }
            return false;
        }

        protected bool Move(List<IComposite> groups, Guid itemPublicKey, Guid? groupKey, Guid? after)
        {
            var moveble = groups.FirstOrDefault(g => g.PublicKey == itemPublicKey);
            if (moveble == null)
                return false;
            if (groupKey.HasValue)
            {
                Group moveToGroup = innerDocument.Find<Group>((Guid) groupKey);
                if (moveToGroup != null)
                {
                    groups.Remove(moveble);
                    moveToGroup.Insert(moveble, after);
                    return true;
                }
            }
            if (!after.HasValue)
            {
                groups.Remove(moveble);
                groups.Insert(0, moveble);
                return true;
            }
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i].PublicKey == after.Value)
                {
                    groups.Remove(moveble);
                    if (i < groups.Count)
                        groups.Insert(i + 1, moveble);
                    else
                        groups.Add(moveble);
                    return true;
                }
            }
            throw new ArgumentException(string.Format("target item doesn't exists -{0}", after));
        }

        public void AddGroup(string groupText,Propagate propageted,List<Guid> triggers,Guid? parent, string conditionExpression)
        {
            Group group = new Group();
            group.Title = groupText;
            group.Propagated = propageted;
            group.Triggers = triggers;
            group.ConditionExpression = conditionExpression;
            try
            {
                Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        public void AddGroup(string groupText, Guid publicKey, Propagate propageted, Guid? parent, string conditionExpression)
        {
            Group group = new Group();
            group.Title = groupText;
            group.Propagated = propageted;
            group.PublicKey = publicKey;
             group.ConditionExpression = conditionExpression;
            try
            {
                Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        public void AddGroup(Guid publicKey, string groupText, Propagate propageted, List<Guid> triggers, Guid? parent, string conditionExpression)
        {
            Group group = new Group();
            group.Title = groupText;
            group.PublicKey = publicKey;
            group.Propagated = propageted;
            group.Triggers = triggers;
            group.ConditionExpression = conditionExpression;
            try
            {
                Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        public void AddGroup(Guid publicKey,string groupText, Propagate propageted, Guid? parent, string conditionExpression)
        {
            Group group = new Group();
            group.PublicKey = publicKey;
            group.Title = groupText;
            group.Propagated = propageted;
            group.ConditionExpression = conditionExpression;
            try
            {
                Add(group, parent);
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Format("group with  publick key {0} can't be found", parent.Value));
            }
        }

        public void UpdateGroup(string groupText, Propagate propageted,List<Guid> triggers, Guid publicKey, string conditionExpression)
        {
            Group group = Find<Group>(publicKey);
            if (group != null)
            {
                group.Propagated = propageted;
                group.Triggers = triggers;
                group.ConditionExpression = conditionExpression;
                group.Update(groupText);
                return;
            }
            throw new ArgumentException(string.Format("group with  publick key {0} can't be found", publicKey));
        }

        public void UpdateGroup(string groupText, Propagate propageted, Guid publicKey, string conditionExpression)
        {
            Group group = Find<Group>(publicKey);
            if (group != null)
            {
                group.Propagated = propageted;
                group.ConditionExpression = conditionExpression;
                group.Update(groupText);
                return;
            }
            throw new ArgumentException(string.Format("group with  publick key {0} can't be found", publicKey));
        }

        QuestionnaireDocument IEntity<QuestionnaireDocument>.GetInnerDocument()
        {
            return this.innerDocument;
        }

        public void UpdateQuestion(Guid publicKey, string text, string stataExportCaption, QuestionType type,
            string condition, string validation, string instructions, bool featured, bool mandatory, Order answerOrder, IEnumerable<Answer> answers)
        {
            var question = Find<AbstractQuestion>(publicKey);
            if (question == null)
                return;
            question.QuestionText = text;
            question.StataExportCaption = stataExportCaption;
            question.QuestionType = type;
            UpdateAnswerList(answers, question);
            question.ConditionExpression = condition;
            question.ValidationExpression = validation;
            question.Instructions = instructions;
            question.Featured = featured;
            question.Mandatory = mandatory;
            question.AnswerOrder = answerOrder;
        }

        public void UpdateConditionExpression(Guid publicKey, string condition)
        {
            var question = Find<AbstractQuestion>(publicKey);
            if (question == null)
                return;
            question.ConditionExpression = condition;
        }

        public Guid PublicKey
        {
            get { return innerDocument.PublicKey; }
        }

        public void Add(IComposite c, Guid? parent)
        {
            innerDocument.Add(c, parent);
        }

        public void Remove(IComposite c)
        {
           innerDocument.Remove(c);
        }

        public void Remove(Guid publicKey)
        {
            innerDocument.Remove(publicKey);
        }

        public T Find<T>(Guid publicKey) where T : class, IComposite
        {
            return innerDocument.Find<T>(publicKey);
        }

        public IEnumerable<T> Find<T>(Func<T, bool> condition) where T : class
        {
            return
                innerDocument.Find<T>(condition);
        }

        public T FirstOrDefault<T>(Func<T, bool> condition) where T : class
        {
            return
              innerDocument.FirstOrDefault<T>(condition);
        }


        public IList<IQuestion> GetAllQuestions()
        {
            return this.innerDocument.GetAllQuestions<IQuestion>().ToList();
        }

        #endregion
    }
}
