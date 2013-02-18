// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupHash.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group hash.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Main.Core.Entities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The group hash.
    /// </summary>
    public class GroupHash
    {
        #region Fields

        /// <summary>
        /// The _hash.
        /// </summary>
        private readonly IDictionary<string, CompleteQuestionWrapper> hash;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupHash"/> class.
        /// </summary>
        public GroupHash()
        {
            this.hash = new Dictionary<string, CompleteQuestionWrapper>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupHash"/> class.
        /// </summary>
        /// <param name="root">
        /// The root.
        /// </param>
        public GroupHash(ICompleteGroup root) : this()
        {
            this.ProcessTree(root);
        }

        #endregion

        #region Public Properties
       
        /// <summary>
        /// Gets the questions.
        /// </summary>
        public IEnumerable<ICompleteQuestion> Questions
        {
            get
            {
                return this.hash.Values.Select(v => v.Question);
            }
        }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        public IEnumerable<CompleteQuestionWrapper> WrapedQuestions
        {
            get
            {
                return this.hash.Values;
            }
        }

        /// <summary>
        /// The get featured questions.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public IEnumerable<ICompleteQuestion> GetFeaturedQuestions()
        {
                return this.hash.Values.Select(v => v.Question).Where(q => q.Featured);
        }
        
        #endregion

        #region Public Indexers

        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public ICompleteQuestion GetQuestion(Guid publicKey, Guid? propagationKey)
        {
            return this.GetQuestionWrapper(publicKey, propagationKey).Question;
        }
       
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add group.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public void AddGroup(ICompleteGroup group)
        {
            
            if (group == null || !group.PropagationPublicKey.HasValue)
            {
                throw new ArgumentException("Only propagated group can uppdate hash.");
            }

            this.ProcessTree(group);
        }

        /// <summary>
        /// The get question.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.Extensions.GroupHash+CompleteQuestionWrapper.
        /// </returns>
        public CompleteQuestionWrapper GetQuestionWrapper(Guid publicKey, Guid? propagationKey)
        {
            if (this.hash.ContainsKey(this.GetQuestionKey(publicKey, null))
                && !this.hash.ContainsKey(this.GetQuestionKey(publicKey, propagationKey)))
            {
                return this.hash[this.GetQuestionKey(publicKey, null)];
            }

            return this.hash[this.GetQuestionKey(publicKey, propagationKey)];
        }

        /// <summary>
        /// The get question by key.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="CompleteQuestionWrapper"/>.
        /// </returns>
        public CompleteQuestionWrapper GetQuestionByKey(string key)
        {
            return this.hash.ContainsKey(key) ? this.hash[key] : null;
        }

        /*/// <summary>
        /// The get question screen.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The <see cref="Guid"/>.
        /// </returns>
        public Guid GetQuestionScreen(Guid publicKey, Guid? propagationKey)
        {
            return this.GetQuestion(publicKey, propagationKey).GroupKey;
        }*/

        /// <summary>
        /// The remove group.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
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
        
        /// <summary>
        /// The get question key.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private string GetQuestionKey(ICompleteQuestion question)
        {
            return this.GetQuestionKey(question.PublicKey, question.PropagationPublicKey);
        }

        /// <summary>
        /// The get question key.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The System.String.
        /// </returns>
        private string GetQuestionKey(Guid publicKey, Guid? propagationKey)
        {
            if (propagationKey.HasValue)
            {
                return string.Format("{0}{1}", publicKey, propagationKey.Value);
            }

            return publicKey.ToString();
        }

        /// <summary>
        /// The process i composite.
        /// </summary>
        /// <param name="node">
        /// The node.
        /// </param>
        /// <param name="nodes">
        /// The nodes.
        /// </param>
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

        /// <summary>
        /// The process tree.
        /// </summary>
        /// <param name="root">
        /// The root.
        /// </param>
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