// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GroupHash.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The group hash.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The group hash.
    /// </summary>
    public class GroupHash
    {
        #region Fields

        /// <summary>
        /// The _hash.
        /// </summary>
        private readonly IDictionary<string, CompleteQuestionWrapper> _hash;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupHash"/> class.
        /// </summary>
        public GroupHash()
        {
            this._hash = new Dictionary<string, CompleteQuestionWrapper>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupHash"/> class.
        /// </summary>
        /// <param name="root">
        /// The root.
        /// </param>
        public GroupHash(ICompleteGroup root)
            : this()
        {
            this.PublicKey = root.PublicKey;
            this.ProcessTree(root);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey { get; private set; }

        /// <summary>
        /// Gets the questions.
        /// </summary>
        public IEnumerable<ICompleteQuestion> Questions
        {
            get
            {
                return this._hash.Values.Select(v => v.Question);
            }
        }
        /// <summary>
        /// Gets the questions.
        /// </summary>
        public IEnumerable<CompleteQuestionWrapper> WrapedQuestions
        {
            get
            {
                return this._hash.Values;
            }
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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public ICompleteQuestion this[Guid publicKey, Guid? propagationKey]
        {
            get
            {
                return this.GetQuestion(publicKey, propagationKey).Question;
            }
        }

        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public ICompleteQuestion this[ICompleteQuestion index]
        {
            get
            {
                return this.GetQuestion(index).Question;
            }
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
            if (!group.PropogationPublicKey.HasValue)
            {
                throw new ArgumentException("only propagated group can uppdate hash");
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
        /// The RavenQuestionnaire.Core.Entities.Extensions.GroupHash+CompleteQuestionWrapper.
        /// </returns>
        public CompleteQuestionWrapper GetQuestion(Guid publicKey, Guid? propagationKey)
        {
            if (this._hash.ContainsKey(this.GetQuestionKey(publicKey, null))
                && !this._hash.ContainsKey(this.GetQuestionKey(publicKey, propagationKey)))
            {
                return this._hash[this.GetQuestionKey(publicKey, null)];
            }

            return this._hash[this.GetQuestionKey(publicKey, propagationKey)];
        }

        /// <summary>
        /// The get question screen.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The System.Guid.
        /// </returns>
        public Guid GetQuestionScreen(Guid publicKey, Guid? propagationKey)
        {
            return this.GetQuestion(publicKey, propagationKey).GroupKey;
        }

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
            if (!group.PropogationPublicKey.HasValue)
            {
                throw new ArgumentException("only propagated group can uppdate hash");
            }

            foreach (string key in this._hash.Keys.ToArray())
            {
                if (key.EndsWith(group.PropogationPublicKey.ToString()))
                {
                    this._hash.Remove(key);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get question.
        /// </summary>
        /// <param name="index">
        /// The index.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Entities.Extensions.GroupHash+CompleteQuestionWrapper.
        /// </returns>
        protected CompleteQuestionWrapper GetQuestion(ICompleteQuestion index)
        {
            if (this._hash.ContainsKey(this.GetQuestionKey(index.PublicKey, null))
                && !this._hash.ContainsKey(this.GetQuestionKey(index.PublicKey, index.PropogationPublicKey)))
            {
                return this._hash[this.GetQuestionKey(index.PublicKey, null)];
            }

            return this._hash[this.GetQuestionKey(index.PublicKey, index.PropogationPublicKey)];
        }

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
            return this.GetQuestionKey(question.PublicKey, question.PropogationPublicKey);
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
            if (node is IBinded)
            {
                return;
            }

            if (question == null)
            {
                var group = node.Node as ICompleteGroup;
                if (group.Propagated != Propagate.None && !group.PropogationPublicKey.HasValue)
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
            if (!this._hash.ContainsKey(questionKey))
            {
                this._hash.Add(questionKey, new CompleteQuestionWrapper(question, node.ParentKey.Value));
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
            /* foreach (IComposite composite in root.Children)
             {*/
            var nodes = new Queue<CompositeWrapper>(new[] { new CompositeWrapper(root, null) });
            while (nodes.Count > 0)
            {
                CompositeWrapper node = nodes.Dequeue();

                this.ProcessIComposite(node, nodes);
            }

            // }
        }

        #endregion

        /// <summary>
        /// The complete question wrapper.
        /// </summary>
        public class CompleteQuestionWrapper
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="CompleteQuestionWrapper"/> class.
            /// </summary>
            /// <param name="question">
            /// The question.
            /// </param>
            /// <param name="screenGuid">
            /// The screen guid.
            /// </param>
            public CompleteQuestionWrapper(ICompleteQuestion question, Guid screenGuid)
            {
                this.Question = question;
                this.GroupKey = screenGuid;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets the group key.
            /// </summary>
            public Guid GroupKey { get; private set; }

            /// <summary>
            /// Gets the question.
            /// </summary>
            public ICompleteQuestion Question { get; private set; }

            #endregion
        }

        /// <summary>
        /// The composite wrapper.
        /// </summary>
        protected class CompositeWrapper
        {
            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="CompositeWrapper"/> class.
            /// </summary>
            /// <param name="node">
            /// The node.
            /// </param>
            /// <param name="parentKey">
            /// The parent key.
            /// </param>
            public CompositeWrapper(IComposite node, Guid? parentKey)
            {
                this.Node = node;
                this.ParentKey = parentKey;
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets the node.
            /// </summary>
            public IComposite Node { get; private set; }

            /// <summary>
            /// Gets the parent key.
            /// </summary>
            public Guid? ParentKey { get; private set; }

            #endregion
        }
    }
}