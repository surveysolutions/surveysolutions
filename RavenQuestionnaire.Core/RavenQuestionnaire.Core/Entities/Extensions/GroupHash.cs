using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public class GroupHash 
    {
        //     private ICompleteGroup root;
      //  private IList<ICompleteQuestion> triggers;
        private IDictionary<string, ICompleteQuestion> hash;
        private IDictionary<string, ICompleteQuestion> triggered;
        private ICompleteQuestion trigger;
        public GroupHash()
        {
            this.hash = new Dictionary<string, ICompleteQuestion>();
            this.triggered = new Dictionary<string, ICompleteQuestion>();
        }

        public GroupHash(ICompleteGroup root):this()
        {
            ProcessTree(root);
        }
        public GroupHash(ICompleteGroup root, ICompleteQuestion trigger):this()
        {
            this.trigger = trigger;
            triggered.Add(GetQuestionKey(trigger), trigger);
            ProcessTree(root);
        }
        private void ProcessTree(ICompleteGroup root)
        {
            Queue<IComposite> nodes = new Queue<IComposite>(new IComposite[1] { root });
            while (nodes.Count > 0)
            {
                IComposite node = nodes.Dequeue();

                ProcessIComposite(node, nodes);

            }
        }

        private void ProcessIComposite(IComposite node,  Queue<IComposite> nodes)
        {
            ICompleteQuestion question = node as ICompleteQuestion;
            if (node is IBinded)
                return;
            if (question == null)
            {
                foreach (IComposite child in node.Children)
                {
                    nodes.Enqueue(child);
                }
                return;
            }
            var questionKey = GetQuestionKey(question);
            hash.Add(questionKey, question);

            if (trigger != null)
            {
                if (!question.Triggers.Contains(trigger.PublicKey))
                    return;
                if (trigger.PropogationPublicKey.HasValue &&
                    trigger.PropogationPublicKey.Value != question.PropogationPublicKey)
                    return;
                if (!triggered.ContainsKey(questionKey))
                    triggered.Add(questionKey, question);
            }
        }

        private string GetQuestionKey(ICompleteQuestion question)
        {
            return GetQuestionKey(question.PublicKey, question.PropogationPublicKey);
        }
        private string GetQuestionKey(Guid publicKey, Guid? propagationKey)
        {
            if (propagationKey.HasValue)
                return string.Format("{0}{1}", publicKey, propagationKey.Value);
            return publicKey.ToString();
        }
        public IEnumerable<ICompleteQuestion> Questions
        {
            get
            {
                if (this.trigger == null)
                    return this.hash.Values;
                return this.triggered.Values;
            }
        }

        public ICompleteQuestion this[Guid publicKey, Guid? propagationKey]
        {
            get
            {
                if (this.hash.ContainsKey(GetQuestionKey(publicKey, null)) && !this.hash.ContainsKey(GetQuestionKey(publicKey, propagationKey)))
                    return this.hash[GetQuestionKey(publicKey, null)];
                return this.hash[GetQuestionKey(publicKey, propagationKey)];
            }
        }
        public ICompleteQuestion this[ICompleteQuestion index]
        {
            get
            {
                if (this.hash.ContainsKey(GetQuestionKey(index.PublicKey, null)) && !this.hash.ContainsKey(GetQuestionKey(index.PublicKey, index.PropogationPublicKey)))
                    return this.hash[GetQuestionKey(index.PublicKey, null)];
                return this.hash[GetQuestionKey(index.PublicKey, index.PropogationPublicKey)];
            }
        }
    }
}
