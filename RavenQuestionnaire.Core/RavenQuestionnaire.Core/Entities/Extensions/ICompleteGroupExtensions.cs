// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompleteGroupExtensions.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The i complete group extensions.
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
    /// The i complete group extensions.
    /// </summary>
    public static class ICompleteGroupExtensions
    {
        #region Public Methods and Operators

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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteGroup.
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
        /// The get all binded questions.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        /// <param name="questionKey">
        /// The question key.
        /// </param>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Entities.SubEntities.Complete.BindedCompleteQuestion].
        /// </returns>
        public static IEnumerable<BindedCompleteQuestion> GetAllBindedQuestions(
            this ICompleteGroup group, Guid questionKey)
        {
            return group.Find<BindedCompleteQuestion>(q => q.ParentPublicKey.Equals(questionKey));
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
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteGroup].
        /// </returns>
        public static IEnumerable<ICompleteGroup> GetPropagatedGroupsByKey(
            this ICompleteGroup entity, Guid propagationKey)
        {
            return entity.Find<ICompleteGroup>(g => g.PropogationPublicKey.Equals(propagationKey));
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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteQuestion.
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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteQuestion.
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
        /// The RavenQuestionnaire.Core.Entities.SubEntities.Complete.ICompleteQuestion.
        /// </returns>
        public static ICompleteQuestion GetRegularQuestion(this ICompleteGroup entity, Guid target)
        {
            var dependency =
                entity.FirstOrDefault<ICompleteQuestion>(
                    q => q.PublicKey.Equals(target) && !q.PropogationPublicKey.HasValue);
            return dependency;
        }

        #endregion

        /*  public static IDictionary<string, ICompleteQuestion> GetAllTriggeredQuestions(this IGroup entity, ICompleteQuestion trigger)
        {
            IDictionary<string, ICompleteQuestion> hash=new Dictionary<string, ICompleteQuestion>();
            IList<ICompleteQuestion> triggers=new List<ICompleteQuestion>();
            if (trigger != null)
            {
                MergeTriggers(triggers, hash, trigger);
            }
            Queue<IComposite> nodes = new Queue<IComposite>(new IComposite[1] {entity});
            while (nodes.Count > 0)
            {
                IComposite node = nodes.Dequeue();

                ProcessIComposite(node, triggers, hash);

                foreach (IComposite child in node.Children)
                {
                    nodes.Enqueue(child);
                }
            }
            return hash;
        }
        static void ProcessIComposite(IComposite node, IList<ICompleteQuestion> triggers, IDictionary<string, ICompleteQuestion> hash)
        {
            ICompleteQuestion question = node as ICompleteQuestion;
            if (node is IBinded)
                return;
            if(question==null)
                return;
            if (triggers.Count() > 0)
            {
                foreach (ICompleteQuestion trigger in triggers)
                {
                    if (!question.Triggers.Contains(trigger.PublicKey))
                        continue;
                    if (trigger.PropogationPublicKey.HasValue &&
                        trigger.PropogationPublicKey.Value != question.PropogationPublicKey)
                        continue;
                }
            }
            MergeTriggers(triggers, hash, question);
        }
        static void MergeTriggers(IList<ICompleteQuestion> triggers, IDictionary<string, ICompleteQuestion> hash, ICompleteQuestion trigger)
        {
            if (!triggers.Contains(trigger))
                triggers.Add(trigger);
            var questionKey = GetQuestionKey(trigger);
            if (!hash.ContainsKey(questionKey))
                hash.Add(questionKey, trigger);
        }

        static string GetQuestionKey(ICompleteQuestion question)
        {
            return question.PublicKey.ToString();
        }*/

        /*
        public static IEnumerable<ICompleteQuestion> GetAllQuestionsFromPropagatedGroup(this ICompleteGroup entity, Guid propagationKey)
        {
            return
                GetPropagatedGroupsByKey(entity, propagationKey).SelectMany(g => g.Questions).Where(q => !(q is IBinded));
        }*/
    }
}