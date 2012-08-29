// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestionnaireScreenIterator.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The questionnaire screen iterator.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Entities.Iterators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The questionnaire screen iterator.
    /// </summary>
    public class QuestionnaireScreenIterator : Iterator<ICompleteGroup>
    {
        #region Fields

        /// <summary>
        /// The groups.
        /// </summary>
        protected IList<ICompleteGroup> groups;

        /// <summary>
        /// The current.
        /// </summary>
        private int current;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireScreenIterator"/> class.
        /// </summary>
        /// <param name="document">
        /// The document.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        public QuestionnaireScreenIterator(IQuestionnaireDocument document)
        {
            List<IComposite> innerGroups = document.Children.Where(c => c is ICompleteGroup).ToList();
            List<IComposite> innerQuestions = document.Children.Where(c => c is ICompleteQuestion).ToList();

            if (document.Children.Count == 0)
            {
                throw new ArgumentException("Questionnaires question list is empty");
            }

            this.groups = new List<ICompleteGroup>(innerGroups.Count + 1);
            if (innerQuestions.Count > 0)
            {
                this.groups.Add(new CompleteGroup { Children = innerQuestions, PublicKey = Guid.Empty });
            }

            foreach (CompleteGroup item in innerGroups)
            {
                this.groups.Add(item);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the current.
        /// </summary>
        public ICompleteGroup Current
        {
            get
            {
                return this.groups[this.current];
            }
        }

        /// <summary>
        /// Gets the next.
        /// </summary>
        public ICompleteGroup Next
        {
            get
            {
                if (!this.MoveNext())
                {
                    return null;
                }

                return this.Current;
            }
        }

        /// <summary>
        /// Gets the previous.
        /// </summary>
        public ICompleteGroup Previous
        {
            get
            {
                if (this.current < 1)
                {
                    return null;
                }

                return this.groups[--this.current];
            }
        }

        #endregion

        #region Explicit Interface Properties

        /// <summary>
        /// Gets the current.
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerator`1[T -&gt; RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteGroup].
        /// </returns>
        public IEnumerator<ICompleteGroup> GetEnumerator()
        {
            return this.groups.GetEnumerator();
        }

        /// <summary>
        /// The move next.
        /// </summary>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public bool MoveNext()
        {
            if (this.current < this.groups.Count - 1)
            {
                this.current++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// The reset.
        /// </summary>
        public void Reset()
        {
            this.current = 0;
        }

        /// <summary>
        /// The set current.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public void SetCurrent(ICompleteGroup item)
        {
            int index = this.groups.IndexOf(item);
            if (index >= 0)
            {
                this.current = index;
            }
            else
            {
                throw new ArgumentOutOfRangeException("groups is absent");
            }
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The System.Collections.IEnumerator.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}