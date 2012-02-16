using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Extensions
{
    public static class CompleteQuestionnaireExtensions
    {
        public static IEnumerable<ICompleteQuestion> GetAllQuestions(this CompleteQuestionnaire entity)
        {
            var groups =
                entity.Find<ICompleteGroup>(
                    g =>
                    (g.Propagated == Propagate.None || g is IPropogate) &&
                    (g is ICompleteGroup<ICompleteGroup, ICompleteQuestion> &&
                     ((ICompleteGroup<ICompleteGroup, ICompleteQuestion>) g).Questions.Count > 0)).Select(
                         g => g as ICompleteGroup<ICompleteGroup, ICompleteQuestion>);
            return
                groups.SelectMany(
                    g => g.Questions).Union(
                        entity.GetInnerDocument().Questions).Where(q => !(q is IBinded));
        }
        public static IEnumerable<ICompleteQuestion> GetAllQuestionsFromPropagatedGroup(this CompleteQuestionnaire entity, Guid propagationKey)
        {
            var propagatedGroups =
                entity.Find<PropagatableCompleteGroup>(
                    g =>
                    g.PropogationPublicKey.Equals(propagationKey));
            return propagatedGroups.SelectMany(g => g.Questions).Where(q => !(q is IBinded));
        }
    }
}
