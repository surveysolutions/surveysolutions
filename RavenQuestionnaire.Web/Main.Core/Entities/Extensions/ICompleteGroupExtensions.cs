namespace Main.Core.Entities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities.Composite;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// The i complete group extensions.
    /// </summary>
    public static class CompleteGroupExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The is group propagation template.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsGroupPropagationTemplate(this ICompleteGroup entity)
        {
            return entity.Propagated != Propagate.None && !entity.PropagationPublicKey.HasValue;
        }

        /// <summary>
        /// The find group by key.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteGroup.
        /// </returns>
        public static ICompleteGroup FindGroupByKey(this ICompleteGroup entity, Guid key, Guid? propagationKey)
        {
            if (!propagationKey.HasValue)
            {
                return entity.Find<ICompleteGroup>(key);
            }

            return entity.GetPropagatedGroupsByKey(propagationKey.Value).FirstOrDefault(g => g.PublicKey.Equals(key));
        }

        /// <summary>
        /// The get all questions.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; T].
        /// </returns>
        public static IEnumerable<T> GetAllQuestions<T>(this IGroup entity) where T : class, IComposite
        {
            var result = new List<T>();
            var groups = new Queue<IComposite>();
            groups.Enqueue(entity);

            while (groups.Count != 0)
            {
                IComposite queueItem = groups.Dequeue();
                var question = queueItem as T;
                if (question != null)
                {
                    result.Add(question);
                    continue;
                }

                foreach (IComposite child in queueItem.Children)
                {
                    groups.Enqueue(child);
                }
            }

            return result;
        }

        /// <summary>
        /// The get propagated groups by key.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Main.Core.Entities.SubEntities.Complete.ICompleteGroup].
        /// </returns>
        public static IEnumerable<ICompleteGroup> GetPropagatedGroupsByKey(
            this ICompleteGroup entity, Guid propagationKey)
        {
            return entity.Find<ICompleteGroup>(g => g.PropagationPublicKey.Equals(propagationKey));
        }

        /// <summary>
        /// The get propagated question.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public static ICompleteQuestion GetPropagatedQuestion(
            this ICompleteGroup group, Guid questionKey, Guid propagationKey)
        {
            IEnumerable<ICompleteGroup> groups = group.GetPropagatedGroupsByKey(propagationKey);
            foreach (ICompleteGroup propagatableCompleteGroup in groups)
            {
                var question =
                    propagatableCompleteGroup.FirstOrDefault<ICompleteQuestion>(q => q.PublicKey == questionKey);
                if (question != null)
                {
                    return question;
                }
            }

            return null;
        }

        /// <summary>
        /// The get question by key.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public static ICompleteQuestion GetQuestionByKey(this ICompleteGroup entity, Guid key, Guid? propagationKey)
        {
            if (!propagationKey.HasValue)
            {
                return entity.GetRegularQuestion(key);
            }

            return entity.GetPropagatedQuestion(key, propagationKey.Value) ?? entity.GetRegularQuestion(key);
        }

        /// <summary>
        /// The get regular question.
        /// </summary>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public static ICompleteQuestion GetRegularQuestion(this ICompleteGroup entity, Guid target)
        {
            var dependency =
                entity.FirstOrDefault<ICompleteQuestion>(
                    q => q.PublicKey.Equals(target) && !q.PropagationPublicKey.HasValue);
            return dependency;
        }

        /// <summary>
        /// The get group title.
        /// </summary>
        /// <param name="doc">
        /// The doc.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetGroupTitle(this ICompleteGroup doc, Guid propagationKey)
        {
            return string.Concat(
                doc.GetPropagatedGroupsByKey(propagationKey)
                .SelectMany(q => q.Children)
                .OfType<ICompleteQuestion>()
                .Where(q => q.Capital)
                .Select(q => q.GetAnswerString() + " "));
        }

        #endregion
    }
}