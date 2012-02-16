using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static IEnumerable<ICompleteQuestion> GetAllQuestions(this ICompleteGroup entity)
        {
            var groups =
                entity.Find<ICompleteGroup>(
                    g =>
                    (g.Propagated == Propagate.None || g is IPropogate) &&
                    (g is ICompleteGroup<ICompleteGroup, ICompleteQuestion> &&
                     ((ICompleteGroup<ICompleteGroup, ICompleteQuestion>)g).Questions.Count > 0)).Select(
                         g => g as ICompleteGroup<ICompleteGroup, ICompleteQuestion>);
            var groupWithQuestions = entity as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
            if (groupWithQuestions != null)
                return
                    groups.SelectMany(
                        g => g.Questions).Union(
                            groupWithQuestions.Questions).Where(q => !(q is IBinded));
            return
                groups.SelectMany(
                    g => g.Questions).Where(q => !(q is IBinded));
        }
/*
        public static IEnumerable<ICompleteQuestion> GetAllQuestionsFromPropagatedGroup(this ICompleteGroup entity, Guid propagationKey)
        {
            return
                GetPropagatedGroupsByKey(entity, propagationKey).SelectMany(g => g.Questions).Where(q => !(q is IBinded));
        }*/
    }
}
