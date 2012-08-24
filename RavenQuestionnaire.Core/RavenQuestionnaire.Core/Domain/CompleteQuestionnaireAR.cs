using Ncqrs;
using System;
using System.Linq;
using Ncqrs.Domain;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Events;
using RavenQuestionnaire.Core.Documents;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Events.Questionnaire.Completed;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// CompleteQuestionnaire Aggregate Root.
    /// </summary>
    public class CompleteQuestionnaireAR : AggregateRootMappedByConvention, ISnapshotable<CompleteQuestionnaireStoreDocument>
    {
        public CompleteQuestionnaireAR(){}

        private CompleteQuestionnaireStoreDocument _doc = new CompleteQuestionnaireStoreDocument();


        public CompleteQuestionnaireAR(Guid completeQuestionnaireId, QuestionnaireDocument questionnaire)
            : base(completeQuestionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            //all checks using read layer.

            //TODO: is it good to use explicit type cast?
            CompleteQuestionnaireDocument doc = (CompleteQuestionnaireDocument)questionnaire;

            doc.PublicKey = completeQuestionnaireId;
            doc.Creator = null;
            doc.Status = SurveyStatus.Initial;
            doc.Responsible = null;

            //was moved to the read time 
            /*var executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
            foreach (IComposite child in doc.Children)
            {
                if (child is IBinded)
                    continue;
                if (child is ICompleteGroup)
                {
                    var group = child as ICompleteGroup;
                    group.Enabled = executor.Execute(group);
                }
                else
                {
                    var question = child as ICompleteQuestion;
                    question.Enabled = executor.Execute(question);
                }
            }*/


            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).

            ApplyEvent(new NewCompleteQuestionnaireCreated
            {
                CompletedQuestionnaireId = completeQuestionnaireId,
                QuestionnaireId = Guid.Parse(questionnaire.Id),
                Questionnaire = doc,
                CreationDate = clock.UtcNow(),
                TotalQuestionCount = doc.Find<ICompleteQuestion>(q => !(q is IBinded)).Count()
            });
        }


        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewCompleteQuestionnaireCreated e)
        {
            _doc = new CompleteQuestionnaireStoreDocument(e.Questionnaire);
        }

        public void SetComment(Guid questionPublickey, string comments, Guid? propogationPublicKey)
        {
            ApplyEvent(new CommentSeted()
                           {
                               Comments = comments,
                               CompleteQuestionnaireId = this._doc.PublicKey,
                               PropogationPublicKey = propogationPublicKey,
                               QuestionPublickey = questionPublickey
                           });
        }
        protected void OnSetCommentCommand(CommentSeted e)
        {
            var questionWrapper = _doc.QuestionHash.GetQuestion(e.QuestionPublickey, e.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if(question==null)
                return;
            
            question.SetComments(e.Comments);
            //_doc.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);

        }
        public void Delete()
        {
            ApplyEvent(new CompleteQuestionnaireDeleted()
                           {
                               CompletedQuestionnaireId = _doc.PublicKey,
                               TemplateId = Guid.Parse(_doc.TemplateId)
                           });
        }
        protected void OnCompleteQuestionnaireDeleted(CompleteQuestionnaireDeleted e)
        {
            _doc = null;
        }

        #region Set Answer

        public void SetAnswer(Guid questionPublicKey, Guid? propogationPublicKey, string completeAnswerValue,
                              List<Guid> completeAnswers)
        {
            //performe check before event raising!!
            var question = _doc.QuestionHash[questionPublicKey, propogationPublicKey];

            //it's not a great idea to build here answer text
            var answerString = "";
            if (completeAnswers == null)
            {
                answerString = completeAnswerValue;
            }
            else
            {
                var answerList = new List<string>();
                foreach (var answerGuid in completeAnswers)
                {
                    var answer = question.Find<ICompleteAnswer>(answerGuid);
                    if (answer != null)
                        answerList.Add(answer.AnswerText);
                }
                answerString = string.Join(", ", answerList.ToArray());
            }

            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnAnswerSet method).
            ApplyEvent(new AnswerSet
                {
                    CompletedQuestionnaireId = this._doc.PublicKey,
                    QuestionPublicKey = questionPublicKey,
                    PropogationPublicKey = propogationPublicKey,
                    Answer = completeAnswers ?? (object) completeAnswerValue,
                    Featured = question.Featured,
                    //clean up this values
                    QuestionText = question.QuestionText,
                    AnswerString = answerString
                });

            //handle group propagation
            //to store events with guids
            if (question is IAutoPropagate)
            {
                int count;
                if (!int.TryParse(completeAnswerValue, out count))
                    return;
                if (count < 0)
                    throw new InvalidOperationException("count can't be bellow zero");

                AddRemovePropagatedGroup(question, count);
            }



        }

        protected void AddRemovePropagatedGroup(ICompleteQuestion question, int count)
        {
            if (!(question is IAutoPropagate))
                return;

            if (count < 0)
                throw new InvalidOperationException("count can't be bellow zero");

            foreach (Guid trigger in question.Triggers)
            {
                MultiplyGroup(trigger, count);
            }
        }

        protected void MultiplyGroup(Guid groupKey, int count)
        {
            var groups =
                this._doc.Find<ICompleteGroup>(
                    g => g.PublicKey == groupKey && g.PropogationPublicKey.HasValue).ToList();

            if (groups.Count == count)
                return;
            if (groups.Count < count)
            {
                for (int i = 0; i < count - groups.Count; i++)
                {
                    ApplyEvent(new PropagatableGroupAdded
                        {
                            CompletedQuestionnaireId = this._doc.PublicKey,
                            PublicKey = groupKey,
                            PropagationKey = Guid.NewGuid()
                        });
                }
            }
            else
            {
                for (int i = count; i < groups.Count; i++)
                {
                    if (!groups[i].PropogationPublicKey.HasValue)
                        continue;

                    ApplyEvent(new PropagatableGroupDeleted
                        {
                            CompletedQuestionnaireId = this._doc.PublicKey,
                            PublicKey = groupKey,
                            PropagationKey = groups[i].PropogationPublicKey.Value
                        });
                }
            }
        }

        // Event handler for the AnswerSet event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnAnswerSet(AnswerSet e)
        {
            var questionWrapper = _doc.QuestionHash.GetQuestion(e.QuestionPublicKey, e.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
                return;
            question.SetAnswer(e.Answer);
           // _doc.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);
        }

        #endregion


        public void DeletePropagatableGroup(Guid propagationKey, Guid publicKey)
        {
            var group = _doc.Find<CompleteGroup>(publicKey);
            ApplyEvent(new PropagatableGroupDeleted
                           {
                               CompletedQuestionnaireId = this._doc.PublicKey,
                               PublicKey = publicKey,
                               PropagationKey = propagationKey
                           });
            if (group.Triggers.Count > 0)
                foreach (Guid trigger in group.Triggers)
                {
                    ApplyEvent(new PropagatableGroupDeleted
                                   {
                                       CompletedQuestionnaireId = this._doc.PublicKey,
                                       PublicKey = trigger,
                                       PropagationKey = propagationKey
                                   });
                }
        }

        // Event handler for the PropagatableGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnPropagatableGroupDeleted(PropagatableGroupDeleted e)
        {
            var group = new CompleteGroup(_doc.Find<CompleteGroup>(e.PublicKey), e.PropagationKey);
            try
            {

           
            _doc.Remove(group);
            _doc.QuestionHash.RemoveGroup(group);
            }
            catch (CompositeException)
            {
                //in case if group was deleted earlier
            }

        }
        public void AddPropagatableGroup(Guid publicKey, Guid propagationKey)
        {
            //performe check before event raising


            var template = _doc.Find<CompleteGroup>(publicKey);


            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnPropagatableGroupAdded method).
            ApplyEvent(new PropagatableGroupAdded
                           {
                               CompletedQuestionnaireId = this._doc.PublicKey,
                               PublicKey = publicKey,
                               PropagationKey = propagationKey
                           });
            if (template.Triggers.Count > 0)
                foreach (Guid trigger in template.Triggers)
                {
                    ApplyEvent(new PropagatableGroupAdded
                                   {
                                       CompletedQuestionnaireId = this._doc.PublicKey,
                                       PublicKey = trigger,
                                       PropagationKey = propagationKey
                                   });
                }
        }

        // Event handler for the PropagatableGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnPropagatableGroupAdded(PropagatableGroupAdded e)
        {
            var template = _doc.Find<CompleteGroup>(e.PublicKey);

            var newGroup = new CompleteGroup(template, e.PropagationKey);
            _doc.Add(newGroup, null);
            _doc.QuestionHash.AddGroup(newGroup);
        }

        #region Implementation of ISnapshotable<CompleteQuestionnaireDocument>

        public CompleteQuestionnaireStoreDocument CreateSnapshot()
        {
            return this._doc;
        }

        public void RestoreFromSnapshot(CompleteQuestionnaireStoreDocument snapshot)
        {
            this._doc = snapshot;
        }

        #endregion

        
        protected void ChangeStatus(SurveyStatus status)
        {
            //put check logic !!!

            CompleteQuestionnaireValidationExecutor validator =
              new CompleteQuestionnaireValidationExecutor(_doc.QuestionHash);

            var result = validator.Execute();
            _doc.IsValid = result;

            ApplyEvent(new QuestionnaireStatusChanged()
            {
                CompletedQuestionnaireId = this._doc.PublicKey,
                Status = result ? status : SurveyStatus.Error
            });
        }

        // Event handler for the PropagatableGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnChangeStatus(QuestionnaireStatusChanged e)
        {
            _doc.Status = e.Status;
        }

        protected void ChangeAssignment(UserLight responsible)
        {
            //put check logic !!!

            ApplyEvent(new QuestionnaireAssignmentChanged()
            {
                CompletedQuestionnaireId = this._doc.PublicKey,
                Responsible = responsible
            });
        }

        // Event handler for the PropagatableGroupAdded event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnChangeAssignment(QuestionnaireAssignmentChanged e)
        {
            _doc.Responsible = e.Responsible;
        }
    }
}
