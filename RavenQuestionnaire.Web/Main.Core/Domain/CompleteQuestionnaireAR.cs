// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireAR.cs" company="">
//   
// </copyright>
// <summary>
//   CompleteQuestionnaire Aggregate Root.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Main.Core.Entities.SubEntities.Complete.Question;

namespace Main.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.Composite;
    using Main.Core.Entities.Extensions;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;
    using Main.Core.Events.Questionnaire.Completed;

    using Ncqrs;
    using Ncqrs.Restoring.EventStapshoot;

    /// <summary>
    /// CompleteQuestionnaire Aggregate Root.
    /// </summary>
    public class CompleteQuestionnaireAR : SnapshootableAggregateRoot<CompleteQuestionnaireDocument>
    {
        #region Fields

        /// <summary>
        /// The doc.
        /// </summary>
        private CompleteQuestionnaireDocument doc = new CompleteQuestionnaireDocument();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireAR"/> class.
        /// </summary>
        public CompleteQuestionnaireAR()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireAR"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        /// The complete questionnaire id.
        /// </param>
        /// <param name="questionnaire">
        /// The questionnaire.
        /// </param>
        public CompleteQuestionnaireAR(Guid completeQuestionnaireId, QuestionnaireDocument questionnaire)
            : base(completeQuestionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            //// all checks using read layer.

            //// TODO: is it good to use explicit type cast?
            var document = (CompleteQuestionnaireDocument)questionnaire;

            document.PublicKey = completeQuestionnaireId;
            document.Creator = null;
            document.Status = SurveyStatus.Initial;
            document.Responsible = null;

            //// Apply a NewQuestionnaireCreated event that reflects the
            //// creation of this instance. The state of this
            //// instance will be update in the handler of 
            //// this event (the OnNewNoteAdded method).
            this.ApplyEvent(
                new NewCompleteQuestionnaireCreated
                    {
                        Questionnaire = document, 
                        CreationDate = clock.UtcNow(), 
                        TotalQuestionCount = document.Find<ICompleteQuestion>(q => !(q is IBinded)).Count()
                    });
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add propagatable group.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        public void AddPropagatableGroup(Guid publicKey, Guid propagationKey)
        {
            //// performe check before event raising
            var template = this.doc.Find<CompleteGroup>(publicKey);

            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnPropagatableGroupAdded method).
            this.ApplyEvent(
                new PropagatableGroupAdded
                    {
                        CompletedQuestionnaireId = this.doc.PublicKey, 
                        PublicKey = publicKey, 
                        PropagationKey = propagationKey
                    });
            if (template.Triggers.Count > 0)
            {
                foreach (Guid trigger in template.Triggers)
                {
                    this.ApplyEvent(
                        new PropagatableGroupAdded
                            {
                                CompletedQuestionnaireId = this.doc.PublicKey, 
                                PublicKey = trigger, 
                                PropagationKey = propagationKey
                            });
                }
            }
        }

        /// <summary>
        /// The create snapshot.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.CompleteQuestionnaireDocument.
        /// </returns>
        public override CompleteQuestionnaireDocument CreateSnapshot()
        {
            return this.doc;
        }

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.

        /// <summary>
        /// The delete.
        /// </summary>
        public void Delete()
        {
            this.ApplyEvent(
                new CompleteQuestionnaireDeleted
                    {
                       CompletedQuestionnaireId = this.doc.PublicKey, TemplateId = this.doc.TemplateId 
                    });
        }

        /// <summary>
        /// The delete propagatable group.
        /// </summary>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        public void DeletePropagatableGroup(Guid propagationKey, Guid publicKey)
        {
            var group = this.doc.Find<CompleteGroup>(publicKey);
            this.ApplyEvent(
                new PropagatableGroupDeleted
                    {
                        CompletedQuestionnaireId = this.doc.PublicKey, 
                        PublicKey = publicKey, 
                        PropagationKey = propagationKey
                    });
            if (group.Triggers.Count > 0)
            {
                foreach (Guid trigger in group.Triggers)
                {
                    this.ApplyEvent(
                        new PropagatableGroupDeleted
                            {
                                CompletedQuestionnaireId = this.doc.PublicKey, 
                                PublicKey = trigger, 
                                PropagationKey = propagationKey
                            });
                }
            }
        }

        /// <summary>
        /// The restore from snapshot.
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot.
        /// </param>
        public override void RestoreFromSnapshot(CompleteQuestionnaireDocument snapshot)
        {
            this.doc = snapshot;
        }

        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        /// <param name="completeAnswerValue">
        /// The complete answer value.
        /// </param>
        /// <param name="completeAnswers">
        /// The complete answers.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Raises InvalidOperationException.
        /// </exception>
        public void SetAnswer(
            Guid questionPublicKey, Guid? propogationPublicKey, string completeAnswerValue, List<Guid> completeAnswers)
        {
            ////performe check before event raising!!
            ICompleteQuestion question = this.doc.QuestionHash[questionPublicKey, propogationPublicKey];

            ////it's not a great idea to build here answer text
            string answerString;
            if (question.IsValueQuestion())
            {
                answerString = completeAnswerValue;
            }
            else
            {
                var answerList = new List<string>();
                foreach (Guid answerGuid in completeAnswers)
                {
                    var answer = question.Find<ICompleteAnswer>(answerGuid);
                    if (answer != null)
                    {
                        answerList.Add(answer.AnswerText);
                    }
                }

                answerString = string.Join(", ", answerList.ToArray());
            }
            ////handle group propagation
            ////to store events with guids
            if (question is IAutoPropagate)
            {
                int count;
                if (!int.TryParse(completeAnswerValue, out count))
                {
                    return;
                }

                ////try to fix empty fields
                if (question is IAutoPropagate)
                {
                    if (string.IsNullOrWhiteSpace(completeAnswerValue))
                    {
                        completeAnswerValue = "0";
                    }
                }

                if (count < 0)
                {
                    throw new InvalidOperationException("count can't be bellow zero");
                }

                this.AddRemovePropagatedGroup(question, count);
            }
            // Apply a NewGroupAdded event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnAnswerSet method).
            this.ApplyEvent(
                new AnswerSet
                    {
                        CompletedQuestionnaireId = this.doc.PublicKey, 
                        QuestionPublicKey = questionPublicKey, 
                        PropogationPublicKey = propogationPublicKey, 
                        AnswerKeys = completeAnswers, 
                        AnswerValue = completeAnswerValue, 
                        Featured = question.Featured, 
                        ////clean up this values
                        QuestionText = question.QuestionText, 
                        AnswerString = answerString
                    });

           
        }

        /// <summary>
        /// The set comment.
        /// </summary>
        /// <param name="questionPublickey">
        /// The question publickey.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propogation public key.
        /// </param>
        public void SetComment(Guid questionPublickey, string comments, Guid? propogationPublicKey)
        {
            this.ApplyEvent(
                new CommentSeted
                    {
                        Comments = comments, 
                        CompleteQuestionnaireId = this.doc.PublicKey, 
                        PropogationPublicKey = propogationPublicKey, 
                        QuestionPublickey = questionPublickey
                    });
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add remove propagated group.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        protected void AddRemovePropagatedGroup(ICompleteQuestion question, int count)
        {
            var autoQuestion = question as AutoPropagateCompleteQuestion;
            var currentAnswer = autoQuestion.Answer ?? 0;
            if (autoQuestion == null)
            {
                return;
            }

            if (count < 0)
            {
                throw new InvalidOperationException("count can't be bellow zero");
            }

            if (currentAnswer == count)
            {
                return;
            }

            if (currentAnswer < count)
            {
                for (int i = 0; i < count - currentAnswer; i++)
                {
                    var propagationKey = Guid.NewGuid();
                    foreach (Guid trigger in autoQuestion.Triggers)
                    {
                        this.ApplyEvent(
                            new PropagatableGroupAdded
                                {
                                    CompletedQuestionnaireId = this.doc.PublicKey,
                                    PublicKey = trigger,
                                    PropagationKey = propagationKey
                                });
                    }
                }
            }
            else
            {
                for (int i = count; i < currentAnswer; i++)
                {
                    foreach (Guid trigger in autoQuestion.Triggers)
                    {
                        Guid trigger1 = trigger;
                        var lastGroup =
                            this.doc.Find<ICompleteGroup>(
                                g => g.PublicKey == trigger1 && g.PropagationPublicKey.HasValue).LastOrDefault();
                        if (lastGroup == null)
                            break;
                        this.ApplyEvent(
                            new PropagatableGroupDeleted
                                {
                                    CompletedQuestionnaireId = this.doc.PublicKey,
                                    PublicKey = trigger,
                                    PropagationKey = lastGroup.PropagationPublicKey.Value
                                });
                    }
                }
            }
        }

        /// <summary>
        /// The change assignment.
        /// </summary>
        /// <param name="responsible">
        /// The responsible.
        /// </param>
        protected void ChangeAssignment(UserLight responsible)
        {
            var prevResponsible = this.doc.Responsible;

            //// put check logic !!!
            this.ApplyEvent(
                new QuestionnaireAssignmentChanged
                    {
                        CompletedQuestionnaireId = this.doc.PublicKey, 
                        PreviousResponsible = prevResponsible, 
                        Responsible = responsible
                    });
        }

        /// <summary>
        /// The change status.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        protected void ChangeStatus(SurveyStatus status)
        {
            this.ApplyEvent(
                new QuestionnaireStatusChanged { CompletedQuestionnaireId = this.doc.PublicKey, Status = status });
        }
        

        // Event handler for the AnswerSet event. This method
        // is automaticly wired as event handler based on convension.

        /// <summary>
        /// The on answer set.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnAnswerSet(AnswerSet e)
        {
            CompleteQuestionWrapper questionWrapper = this.doc.QuestionHash.GetQuestion(
                e.QuestionPublicKey, e.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return; ////is it good or exception is better decision
            }

            question.SetAnswer(e.AnswerKeys, e.AnswerValue);

            // _doc.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);
        }

        // Event handler for the PropagatableGroupAdded event. This method
        // is automaticly wired as event handler based on convension.

        /// <summary>
        /// On Change Assignment.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnChangeAssignment(QuestionnaireAssignmentChanged e)
        {
            this.doc.Responsible = e.Responsible;
        }

        /// <summary>
        /// The on change status.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnChangeStatus(QuestionnaireStatusChanged e)
        {
            this.doc.Status = e.Status;
        }

        /// <summary>
        /// The on complete questionnaire deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnCompleteQuestionnaireDeleted(CompleteQuestionnaireDeleted e)
        {
            this.doc = null;
        }

        /// <summary>
        /// The on new questionnaire created.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewQuestionnaireCreated(NewCompleteQuestionnaireCreated e)
        {
            this.doc = e.Questionnaire;
        }

        /// <summary>
        /// The on propagatable group added.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnPropagatableGroupAdded(PropagatableGroupAdded e)
        {
            var template = this.doc.Find<CompleteGroup>(e.PublicKey);

            var newGroup = new CompleteGroup(template, e.PropagationKey);
            this.doc.Add(newGroup, null);
            this.doc.QuestionHash.AddGroup(newGroup);
        }

        /// <summary>
        /// The on propagatable group deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnPropagatableGroupDeleted(PropagatableGroupDeleted e)
        {
            var group = new CompleteGroup(this.doc.Find<CompleteGroup>(e.PublicKey), e.PropagationKey);
            try
            {
                this.doc.Remove(group);
                this.doc.QuestionHash.RemoveGroup(group);
            }
            catch (CompositeException)
            {
                //// in case if group was deleted earlier
            }
        }

        /// <summary>
        /// The on set comment command.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnSetCommentCommand(CommentSeted e)
        {
            CompleteQuestionWrapper questionWrapper = this.doc.QuestionHash.GetQuestion(
                e.QuestionPublickey, e.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
            }

            question.SetComments(e.Comments);

            //// _doc.LastVisitedGroup = new VisitedGroup(questionWrapper.GroupKey, question.PropogationPublicKey);
        }

        #endregion
    }
}