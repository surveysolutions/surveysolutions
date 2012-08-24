using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public static class ICompleteGroupExtensions
    {
        public static IEnumerable<ICompleteGroup> GetPropagatedGroupsByKey(this ICompleteGroup entity, Guid propagationKey)
        {
            return
                entity.Find<ICompleteGroup>(g => g.PropogationPublicKey.Equals(propagationKey));
        }

        public static IEnumerable<BindedCompleteQuestion> GetAllBindedQuestions(this ICompleteGroup group, Guid questionKey)
        {
            return
                group.Find<BindedCompleteQuestion>(
                    q => q.ParentPublicKey.Equals(questionKey));

        }
        public static ICompleteQuestion GetPropagatedQuestion(this ICompleteGroup group, Guid questionKey, Guid propagationKey)
        {
            var groups = group.GetPropagatedGroupsByKey(propagationKey);
            foreach (ICompleteGroup propagatableCompleteGroup in groups)
            {
                var question =
                    propagatableCompleteGroup.FirstOrDefault<ICompleteQuestion>(q => q.PublicKey == questionKey);
                if (question != null)
                    return question;
            }
            return null;

        }
        public static ICompleteQuestion GetRegularQuestion(this ICompleteGroup entity, Guid target)
        {
            var dependency = entity.FirstOrDefault<ICompleteQuestion>(
                q => q.PublicKey.Equals(target) && !q.PropogationPublicKey.HasValue);
            return dependency;
        }

        public static IEnumerable<T> GetAllQuestions<T>(this IGroup entity) where T: class, IComposite 
        {
            List<T> result = new List<T>();
            Queue<IComposite> groups = new Queue<IComposite>();
            groups.Enqueue(entity);

            while (groups.Count != 0)
            {
                var queueItem = groups.Dequeue();
                var question = queueItem as T;
                if (question != null)
                {

                    result.Add(question);
                    continue;
                }
                foreach (var child in queueItem.Children)
                {
                    groups.Enqueue(child);
                }
            }
            return result;
        }

        public static ICompleteQuestion GetQuestionByKey(this ICompleteGroup entity, Guid key, Guid? propagationKey)
        {
            if (!propagationKey.HasValue)
            {
                return
                    entity.GetRegularQuestion(key);

            }
            return
                entity.GetPropagatedQuestion(key,
                                             propagationKey.Value) ??
                entity.GetRegularQuestion(key);
        }

        public static ICompleteGroup FindGroupByKey(this ICompleteGroup entity, Guid key, Guid? propagationKey)
        {
            if (!propagationKey.HasValue)
                return entity.Find<ICompleteGroup>(key);
            return entity.GetPropagatedGroupsByKey(propagationKey.Value).FirstOrDefault(g => g.PublicKey.Equals(key));
        }

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
