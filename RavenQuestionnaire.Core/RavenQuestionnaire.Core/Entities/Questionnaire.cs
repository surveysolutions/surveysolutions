using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities
{
    public class Questionnaire : IEntity<QuestionnaireDocument>
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
            innerDocument.Children.RemoveAll(a=>a is IQuestion);
        }

        public AbstractQuestion AddQuestion(string text, string stataExportCaption, QuestionType type, string condition, string validation, bool featured, Order answerOrder, Guid? groupPublicKey,
            IEnumerable<Answer> answers)
        {

            var result = new CompleteQuestionFactory().Create(type);
            result.QuestionText = text;
            result.StataExportCaption = stataExportCaption;
            result.ConditionExpression = condition;
            result.ValidationExpression = validation;
            result.AnswerOrder = answerOrder;
            result.Featured = featured;
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

        public void MoveItem(Guid itemPublicKey, Guid? after)
        {
            var result= MoveItem(this.innerDocument, itemPublicKey, after);
            if(!result)
                throw new ArgumentException(string.Format("item doesn't exists -{0}", itemPublicKey));
        }
        protected bool MoveItem(IComposite root, Guid itemPublicKey, Guid? after)
        {
            if (Move(root.Children, itemPublicKey, after))
                return true;

            foreach (IComposite group in root.Children)
            {
                if (MoveItem(group, itemPublicKey, after))
                    return true;
            }
            return false;
        }

        protected bool Move(List<IComposite> groups, Guid itemPublicKey, Guid? after)
        {
            var moveble = groups.FirstOrDefault(g => g.PublicKey == itemPublicKey);
            if (moveble == null)
                return false;
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
                  /*  int movableIndex = groups.IndexOf(moveble);
                    var temp = groups[i];
                    groups[i] = moveble;
                    groups[movableIndex] = temp;*/
                       groups.Remove(moveble);
                    
                    if (i < groups.Count)
                        groups.Insert(i + 1, moveble);
                    else
                        groups.Add(moveble);
                 //   groups.RemoveAt(movableIndex);
                    return true;
                }
            }
            throw new ArgumentException(string.Format("target item doesn't exists -{0}", after));
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
        public void UpdateQuestion(Guid publicKey, string text, string stataExportCaption, QuestionType type,
            string condition, string validation, string instructions, bool featured, Order answerOrder, IEnumerable<Answer> answers)
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


        #region Implementation of IObservable<out CompositeEventArgs>

        public IDisposable Subscribe(IObserver<CompositeEventArgs> observer)
        {
            return innerDocument.Subscribe(observer);
        }

        #endregion
    }
}
