// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireAR.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   CompleteQuestionnaire Aggregate Root.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Ncqrs;
    using Ncqrs.Domain;
    using Ncqrs.Eventing.Sourcing.Snapshotting;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.Composite;
    using RavenQuestionnaire.Core.Entities.Extensions;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
    using RavenQuestionnaire.Core.Events;
    using RavenQuestionnaire.Core.Events.Questionnaire.Completed;
    using RavenQuestionnaire.Core.ExpressionExecutors;

    /// <summary>
    /// CompleteQuestionnaire Aggregate Root.
    /// </summary>
    public class CompleteQuestionnaireAR : AggregateRootMappedByConvention, ISnapshotable<CompleteQuestionnaireDocument>
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
        public CompleteQuestionnaireDocument CreateSnapshot()
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
        public void RestoreFromSnapshot(CompleteQuestionnaireDocument snapshot)
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
            if (completeAnswers == null)
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

            ////handle group propagation
            ////to store events with guids
            if (question is IAutoPropagate)
            {
                int count;
                if (!int.TryParse(completeAnswerValue, out count))
                {
                    return;
                }

                if (count < 0)
                {
                    throw new InvalidOperationException("count can't be bellow zero");
                }

                this.AddRemovePropagatedGroup(question, count);
            }
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
            if (!(question is IAutoPropagate))
            {
                return;
            }

            if (count < 0)
            {
                throw new InvalidOperationException("count can't be bellow zero");
            }

            foreach (Guid trigger in question.Triggers)
            {
                this.MultiplyGroup(trigger, count);
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
                new QuestionnaireStatusChanged
                    {
                        CompletedQuestionnaireId = this.doc.PublicKey,
                        Status = status
                    });
        }

        /// <summary>
        /// The multiply group.
        /// </summary>
        /// <param name="groupKey">
        /// The group key.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        protected void MultiplyGroup(Guid groupKey, int count)
        {
            List<ICompleteGroup> groups =
                this.doc.Find<ICompleteGroup>(g => g.PublicKey == groupKey && g.PropogationPublicKey.HasValue).ToList();

            if (groups.Count == count)
            {
                return;
            }

            if (groups.Count < count)
            {
                for (int i = 0; i < count - groups.Count; i++)
                {
                    this.ApplyEvent(
                        new PropagatableGroupAdded
                            {
                                CompletedQuestionnaireId = this.doc.PublicKey, 
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
                    {
                        continue;
                    }

                    this.ApplyEvent(
                        new PropagatableGroupDeleted
                            {
                                CompletedQuestionnaireId = this.doc.PublicKey, 
                                PublicKey = groupKey, 
                                PropagationKey = groups[i].PropogationPublicKey.Value
                            });
                }
            }
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
            GroupHash.CompleteQuestionWrapper questionWrapper = this.doc.QuestionHash.GetQuestion(
                e.QuestionPublicKey, e.PropogationPublicKey);
            ICompleteQuestion question = questionWrapper.Question;
            if (question == null)
            {
                return;
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
        /// On Propagatable Group Added.
        /// </summary>
        /// The e.
        /// <param name="e">
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
            GroupHash.CompleteQuestionWrapper questionWrapper = this.doc.QuestionHash.GetQuestion(
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