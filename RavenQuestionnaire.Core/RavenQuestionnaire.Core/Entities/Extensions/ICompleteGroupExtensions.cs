using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public static class ICompleteGroupExtensions
    {
        public static IEnumerable<PropagatableCompleteGroup> GetPropagatedGroupsByKey(this ICompleteGroup entity, Guid propagationKey)
        {
            return
                entity.Find<PropagatableCompleteGroup>(g => g.PropogationPublicKey.Equals(propagationKey));
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
            foreach (PropagatableCompleteGroup propagatableCompleteGroup in groups)
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
                q => q.PublicKey.Equals(target) && !(q is IPropogate));
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

/*
        public static IEnumerable<ICompleteQuestion> GetAllQuestionsFromPropagatedGroup(this ICompleteGroup entity, Guid propagationKey)
        {
            return
                GetPropagatedGroupsByKey(entity, propagationKey).SelectMany(g => g.Questions).Where(q => !(q is IBinded));
        }*/
    }
}
