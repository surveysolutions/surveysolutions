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

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public class PropagationSubscriber : EntitySubscriber<ICompleteGroup>
    {
        #region Overrides of EntitySubscriber<ICompleteGroup>

        public override void Subscribe(ICompleteGroup target)
        {
            target.GetGroupPropagatedEvents().Subscribe(Observer.Create<CompositeAddedEventArgs>((e) =>
            {
                PropagatableCompleteGroup group = e.AddedComposite as PropagatableCompleteGroup;
                if (group == null)
                    return;
                var triggeres =
                    target.Find<ICompleteGroup>(
                        g => g.Triggers.Count(gp => gp.Equals(group.PublicKey)) > 0).ToList();
                foreach (ICompleteGroup triggere in triggeres)
                {
                    var propagatebleGroup = new PropagatableCompleteGroup(triggere, group.PropogationPublicKey);
                    target.Add(propagatebleGroup, null);
                }
            }));
            target.GetGroupPropagatedRemovedEvents().Subscribe(Observer.Create<CompositeRemovedEventArgs>((e)=>
                                                                                                              {
                                                                                                                  PropagatableCompleteGroup group = e.RemovedComposite as PropagatableCompleteGroup;
                                                                                                                  if (group == null)
                                                                                                                      return;
                                                                                                                  var triggeres =
                                                                                                                    target.Find<ICompleteGroup>(
                                                                                                                        g => g.Triggers.Count(gp => gp.Equals(group.PublicKey)) > 0).ToList();
                                                                                                                  foreach (ICompleteGroup triggere in triggeres)
                                                                                                                  {
                                                                                                                      var propagatebleGroup = new PropagatableCompleteGroup(triggere, group.PropogationPublicKey);
                                                                                                                      target.Remove(propagatebleGroup);
                                                                                                                  }
                                                                                                              }));

            var addAnswers = from q in target.GetAllAnswerAddedEvents()
                             let question =
                                 ((CompositeAddedEventArgs)q.ParentEvent).AddedComposite as
                                 ICompleteQuestion
                             where question.QuestionType == QuestionType.AutoPropagate
                             select q;

            addAnswers
                .Subscribe(Observer.Create<CompositeAddedEventArgs>(
                    (e) =>
                        {
                            var question = ((CompositeAddedEventArgs) e.ParentEvent).AddedComposite as ICompleteQuestion;

                            if (question == null || question.QuestionType != QuestionType.AutoPropagate)
                                return;

                            var countObj = question.GetValue();

                            int count = Convert.ToInt32(countObj);

                            if (count < 0)
                                throw new InvalidOperationException("caount can't be bellow zero");
                            if (!question.Attributes.ContainsKey("TargetGroupKey"))
                                return;
                            Guid targetGroupKey = Guid.Parse(question.Attributes["TargetGroupKey"].ToString());
                            var groups =
                                target.Find<PropagatableCompleteGroup>(g => g.PublicKey == targetGroupKey).ToList();
                            if (groups.Count == count)
                                return;
                            if (groups.Count < count)
                            {
                                var template =
                                    target.Find<ICompleteGroup>(
                                        g => g.PublicKey == targetGroupKey && !(g is PropagatableCompleteGroup)).
                                        FirstOrDefault();
                                for (int i = 0; i < count - groups.Count; i++)
                                {
                                    target.Add(new PropagatableCompleteGroup(template, Guid.NewGuid()), null);
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
