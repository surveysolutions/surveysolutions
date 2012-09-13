using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Entities.Subscribers
{
    public class BindedQuestionSubscriber : EntitySubscriber<ICompleteGroup>
    {
        #region Implementation of IDisposable

        protected override IDisposable GetUnsubscriber(ICompleteGroup target)
        
        {
            var addAnswers = from q in target.GetAllQuestionAnsweredEvents()
                             let question =
                                q.AddedComposite as
                                 ICompleteQuestion
                             let binded =
                                 target.GetAllBindedQuestions(question.PublicKey)
                             where binded.Any()
                             select q;
             return addAnswers
                                                 .Subscribe(Observer.Create<CompositeAddedEventArgs>(
                                                     (e) =>
                                                         {
                                                             var template =e.AddedComposite as ICompleteQuestion;

                                                             if (template == null)
                                                                 return;
                                                           //  var propagatedTemplate = template as IPropogate;
                                                             IEnumerable<BindedCompleteQuestion> binded;
                                                             if (!template.PropogationPublicKey.HasValue)
                                                             {
                                                                 binded =
                                                                     target.GetAllBindedQuestions(template.PublicKey);
                                                             }
                                                             else
                                                             {
                                                                 binded = target.GetPropagatedGroupsByKey(
                                                                     template.PropogationPublicKey.Value).
                                                                     SelectMany(
                                                                         pg =>
                                                                         pg.GetAllBindedQuestions(template.PublicKey));
                                                             }
                                                             foreach (
                                                                 BindedCompleteQuestion bindedCompleteQuestion in binded
                                                                 )
                                                             {
                                                                 bindedCompleteQuestion.Copy(template);
                                                             }

                                                         }));


        }

        #endregion


    }
}
