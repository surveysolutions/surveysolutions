namespace Main.Core.Entities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    public class GroupHash
    {
        private readonly IDictionary<string, CompleteQuestionWrapper> hash;

        public GroupHash()
        {
            this.hash = new Dictionary<string, CompleteQuestionWrapper>();
        }

        public GroupHash(ICompleteGroup root) : this()
        {
            this.ProcessTree(root);
        }

        #region Public Properties
       
        public IEnumerable<ICompleteQuestion> Questions
        {
            get
            {
                return this.hash.Values.Select(v => v.Question);
            }
        }

        public IEnumerable<CompleteQuestionWrapper> WrapedQuestions
        {
            get
            {
                return this.hash.Values;
            }
        }

        public IEnumerable<ICompleteQuestion> GetFeaturedQuestions()
        {
                return this.hash.Values.Select(v => v.Question).Where(q => q.Featured);
        }
        
        #endregion

        #region Public Indexers

        public ICompleteQuestion GetQuestion(Guid publicKey, Guid? propagationKey)
        {
            return this.GetQuestionWrapper(publicKey, propagationKey).Question;
        }
       
        #endregion

        #region Public Methods and Operators

        public void AddGroup(ICompleteGroup group)
        {
            
            if (group == null || !group.PropagationPublicKey.HasValue)
            {
                throw new ArgumentException("Only propagated group can uppdate hash.");
            }

            this.ProcessTree(group);
        }

        public CompleteQuestionWrapper GetQuestionWrapper(Guid publicKey, Guid? propagationKey)
        {
            if (this.hash.ContainsKey(this.GetQuestionKey(publicKey, null))
                && !this.hash.ContainsKey(this.GetQuestionKey(publicKey, propagationKey)))
            {
                return this.hash[this.GetQuestionKey(publicKey, null)];
            }

            return this.hash[this.GetQuestionKey(publicKey, propagationKey)];
        }

        public CompleteQuestionWrapper GetQuestionByKey(string key)
        {
            return this.hash.ContainsKey(key) ? this.hash[key] : null;
        }

        /*public Guid GetQuestionScreen(Guid publicKey, Guid? propagationKey)
        {
            return this.GetQuestion(publicKey, propagationKey).GroupKey;
        }*/

        public void RemoveGroup(ICompleteGroup group)
        {
            if (group == null)
            {
                return;
            }

            if (!group.PropagationPublicKey.HasValue)
            {
                throw new ArgumentException("Only propagated group can uppdate hash.");
            }

            foreach (string key in this.hash.Keys.ToArray())
            {
                if (key.EndsWith(group.PropagationPublicKey.ToString()))
                {
                    this.hash.Remove(key);
                }
            }
        }

        #endregion

        #region Methods
        
        private string GetQuestionKey(ICompleteQuestion question)
        {
            return this.GetQuestionKey(question.PublicKey, question.PropagationPublicKey);
        }

        private string GetQuestionKey(Guid publicKey, Guid? propagationKey)
        {
            if (propagationKey.HasValue)
            {
                return string.Format("{0}{1}", publicKey, propagationKey.Value);
            }

            return publicKey.ToString();
        }

        private void ProcessIComposite(CompositeWrapper node, Queue<CompositeWrapper> nodes)
        {
            var question = node.Node as ICompleteQuestion;

            if (question == null)
            {
                var group = node.Node as ICompleteGroup;
                if (group.IsGroupPropagationTemplate())
                {
                    return;
                }

                foreach (IComposite child in node.Node.Children)
                {
                    nodes.Enqueue(new CompositeWrapper(child, node.Node.PublicKey));
                }

                return;
            }

            string questionKey = this.GetQuestionKey(question);
            if (!this.hash.ContainsKey(questionKey))
            {
                this.hash.Add(questionKey, new CompleteQuestionWrapper(question, node.ParentKey.Value));
            }
        }

        private void ProcessTree(ICompleteGroup root)
        {
            var nodes = new Queue<CompositeWrapper>(new[] { new CompositeWrapper(root, null) });
            while (nodes.Count > 0)
            {
                CompositeWrapper node = nodes.Dequeue();

                this.ProcessIComposite(node, nodes);
            }
        }

        #endregion
    }
}