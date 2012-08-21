using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public class GroupHash 
    {
        //     private ICompleteGroup root;
      //  private IList<ICompleteQuestion> triggers;
        private IDictionary<string, CompleteQuestionWrapper> hash;
        public GroupHash()
        {
            this.hash = new Dictionary<string, CompleteQuestionWrapper>();
        }

        public GroupHash(ICompleteGroup root):this()
        {
            this.PublicKey = root.PublicKey;
            ProcessTree(root);
        }
        public void AddGroup(ICompleteGroup group)
        {
            if (!group.PropogationPublicKey.HasValue)
                throw new ArgumentException("only propagated group can uppdate hash");
            ProcessTree(group);
        }
        public void RemoveGroup(ICompleteGroup group)
        {
            if (!group.PropogationPublicKey.HasValue)
                throw new ArgumentException("only propagated group can uppdate hash");
            foreach (string key in hash.Keys.ToArray())
            {
                if (key.EndsWith(group.PropogationPublicKey.ToString()))
                    hash.Remove(key);
            }
        }

        private void ProcessTree(ICompleteGroup root)
        {
            /* foreach (IComposite composite in root.Children)
             {*/
            Queue<CompositeWrapper> nodes = new Queue<CompositeWrapper>(new CompositeWrapper[1] { new CompositeWrapper(root,null) });
            while (nodes.Count > 0)
            {
                CompositeWrapper node = nodes.Dequeue();

                ProcessIComposite(node, nodes);

            }
            // }
        }

        private void ProcessIComposite(CompositeWrapper node, Queue<CompositeWrapper> nodes)
        {
            ICompleteQuestion question = node.Node as ICompleteQuestion;
            if (node is IBinded)
                return;

            if (question == null)
            {
                var group = node.Node as ICompleteGroup;
                if (group.Propagated != Propagate.None && !group.PropogationPublicKey.HasValue)
                    return;
                foreach (IComposite child in node.Node.Children)
                {
                    nodes.Enqueue(new CompositeWrapper(child, node.Node.PublicKey));
                }
                return;
            }
            var questionKey = GetQuestionKey(question);
            if (!hash.ContainsKey(questionKey))
                hash.Add(questionKey, new CompleteQuestionWrapper(question, node.ParentKey.Value));

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
            get { return this.hash.Values.Select(v=>v.Question); }
        }
        public CompleteQuestionWrapper GetQuestion(Guid publicKey, Guid? propagationKey)
        {
            if (this.hash.ContainsKey(GetQuestionKey(publicKey, null)) && !this.hash.ContainsKey(GetQuestionKey(publicKey, propagationKey)))
                return this.hash[GetQuestionKey(publicKey, null)];
            return this.hash[GetQuestionKey(publicKey, propagationKey)];
        }
        protected CompleteQuestionWrapper GetQuestion(ICompleteQuestion index)
        {
            if (this.hash.ContainsKey(GetQuestionKey(index.PublicKey, null)) && !this.hash.ContainsKey(GetQuestionKey(index.PublicKey, index.PropogationPublicKey)))
                return this.hash[GetQuestionKey(index.PublicKey, null)];
            return this.hash[GetQuestionKey(index.PublicKey, index.PropogationPublicKey)];
        }
        public ICompleteQuestion this[Guid publicKey, Guid? propagationKey]
        {
            get
            {
                return GetQuestion(publicKey, propagationKey).Question;
            }
        }
        public ICompleteQuestion this[ICompleteQuestion index]
        {
            get { return GetQuestion(index).Question; }
        }
        public Guid GetQuestionScreen(Guid publicKey, Guid? propagationKey)
        {
            return GetQuestion(publicKey, propagationKey).GroupKey;
        }

        public Guid PublicKey { get; private set; }

        public class CompleteQuestionWrapper
        {
            public CompleteQuestionWrapper(ICompleteQuestion question, Guid screenGuid)
            {
                Question = question;
                GroupKey = screenGuid;
            }

            public ICompleteQuestion Question { get; private set; }
            public Guid GroupKey { get; private set; }
        }
        protected class CompositeWrapper
        {
            public CompositeWrapper(IComposite node, Guid? parentKey)
            {
                Node = node;
                ParentKey = parentKey;
            }

            public IComposite Node { get; private set; }
            public Guid? ParentKey { get; private set; }
        }
    }
}
