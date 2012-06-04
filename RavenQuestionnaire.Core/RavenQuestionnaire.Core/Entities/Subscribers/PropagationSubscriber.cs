using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public class PropagationSubscriber : EntitySubscriber<ICompleteGroup>
    {
        #region Overrides of EntitySubscriber<ICompleteGroup>

        public override void Subscribe(ICompleteGroup target)
        {
            target.GetGroupPropagatedEvents().Subscribe(Observer.Create<CompositeAddedEventArgs>((e) =>
            {
                ICompleteGroup group = e.AddedComposite as ICompleteGroup;
                if (group == null || !group.PropogationPublicKey.HasValue)
                    return;
                var triggeres =
                    target.Find<ICompleteGroup>(
                        g => g.Triggers.Count(gp => gp == group.PublicKey) > 0).ToList();
                foreach (ICompleteGroup triggere in triggeres)
                {
                    var propagatebleGroup = new CompleteGroup(triggere, group.PropogationPublicKey.Value);
                    target.Add(propagatebleGroup, null);
                }
            }));
            target.GetGroupPropagatedRemovedEvents().Subscribe(Observer.Create<CompositeRemovedEventArgs>((e)=>
                                                                                                              {
                                                                                                                  ICompleteGroup group = e.RemovedComposite as ICompleteGroup;
                                                                                                                  if (group == null || !group.PropogationPublicKey.HasValue)
                                                                                                                      return;
                                                                                                                  var triggeres =
                                                                                                                    target.Find<ICompleteGroup>(
                                                                                                                        g => g.Triggers.Count(gp => gp.Equals(group.PublicKey)) > 0).ToList();
                                                                                                                  foreach (ICompleteGroup triggere in triggeres)
                                                                                                                  {
                                                                                                                      var propagatebleGroup = new CompleteGroup(triggere, group.PropogationPublicKey.Value);
                                                                                                                      target.Remove(propagatebleGroup);
                                                                                                                  }
                                                                                                              }));

            var addAnswers = from q in target.GetAllQuestionAnsweredEvents()
                             where q.AddedComposite is IAutoPropagate
                             select q;

            addAnswers
                .Subscribe(Observer.Create<CompositeAddedEventArgs>(
                    (e) =>
                        {
                            var question = e.AddedComposite as AutoPropagateCompleteQuestion;

                            if (question == null)
                                return;

                            var countObj = question.GetAnswerObject();

                            int count = Convert.ToInt32(countObj);

                            if (count < 0)
                                throw new InvalidOperationException("caount can't be bellow zero");

                            Guid targetGroupKey = question.TargetGroupKey;
                            var groups =
                                target.Find<ICompleteGroup>(g => g.PublicKey == targetGroupKey && g.PropogationPublicKey.HasValue).ToList();
                            if (groups.Count == count)
                                return;
                            if (groups.Count < count)
                            {
                                var template =
                                    target.Find<ICompleteGroup>(
                                        g => g.PublicKey == targetGroupKey && !g.PropogationPublicKey.HasValue).
                                        FirstOrDefault();
                                for (int i = 0; i < count - groups.Count; i++)
                                {
                                    target.Add(new CompleteGroup(template, Guid.NewGuid()), null);
                                }
                            }
                            else
                            {
                                for (int i = count; i < groups.Count; i++)
                                {
                                    target.Remove(groups[i]);
                                }
                            }

                        }));
        }

        #endregion
    }
}
