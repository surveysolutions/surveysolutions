using Main.Core.Domain.Exceptions;
using Ncqrs.Domain;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.SharedKernel.Structures.Synchronization;

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
    using Main.Core.Entities.SubEntities.Complete.Question;
    using Main.Core.Events.Questionnaire.Completed;
    using Main.Core.ExpressionExecutors;

    using Ncqrs;

    /// <summary>
    /// CompleteQuestionnaire Aggregate Root.
    /// </summary>
    public class CompleteQuestionnaireAR : AggregateRootMappedByConvention, ISnapshotable<CompleteQuestionnaireDocument>
    {
        #region Fields

        /// <summary>
        /// The doc.
        /// </summary>
        private CompleteQuestionnaireDocument document = new CompleteQuestionnaireDocument();
        
        private CompleteQuestionnaireDocument doc
        {
            get { return document; }
            set { document = value.Clone() as CompleteQuestionnaireDocument; }
        }
    
        
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireAR"/> class.
        /// </summary>
        public CompleteQuestionnaireAR()
        {
        }

        public CompleteQuestionnaireAR(Guid publicKey, Guid templateId, string title, Guid? responsibleId, Guid statusId, List<FeaturedQuestionMeta> featuredAnswers)
            : base(publicKey)
        {
            UpdateInterviewMetaInfo(templateId, title, responsibleId, statusId, featuredAnswers);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireAR"/> class.
        /// </summary>
        /// <param name="completeQuestionnaireId">
        ///   The complete questionnaire id.
        /// </param>
        /// <param name="questionnaire">
        ///   The questionnaire.
        /// </param>
        /// <param name="creator"></param>
        public CompleteQuestionnaireAR(Guid completeQuestionnaireId, 
            QuestionnaireDocument questionnaire, UserLight creator)
            : base(completeQuestionnaireId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            //// all checks using read layer.

            //// TODO: think: is it good to use explicit type cast?
            var document = (CompleteQuestionnaireDocument)questionnaire;

            //// connections are set during type casting
            //// document.ConnectChildsWithParent();

            document.PublicKey = completeQuestionnaireId;
            document.Creator = creator;
            document.Status = SurveyStatus.Unknown;
            document.Responsible = null;

            ////document.ConnectChildsWithParent();

            //// Apply a NewQuestionnaireCreated event that reflects the
            //// creation of this instance. The state of this
            //// instance will be update in the handler of 
            //// this event (the OnNewNoteAdded method).
            this.ApplyEvent(
                new NewCompleteQuestionnaireCreated
                    {
                        Questionnaire = document, // to avoid pass as refecence 
                        CreationDate = clock.UtcNow(), 
                        TotalQuestionCount = document.Find<ICompleteQuestion>(q => true).Count()
                    });
        }

        public CompleteQuestionnaireAR(Guid interviewId, QuestionnaireDocument questionnaire, UserLight creator, UserLight responsible, List<QuestionAnswer> featuredAnswers)
            : this(interviewId, questionnaire, creator)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            this.ChangeAssignment(responsible);
            foreach (var featuredAnswer in featuredAnswers)
            {
                this.SetAnswer(featuredAnswer.Id, null, featuredAnswer.Answer, featuredAnswer.Answers.ToList(),
                               clock.UtcNow());
            }
            #warning Madagaskar fix. Should be discussed
            this.ChangeStatus(SurveyStatus.Unassign, responsible);
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The add propagate group.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        public void AddPropagatableGroup(Guid publicKey, Guid propagationKey)
        {
            throw new InvalidOperationException("Is not supported any more.");

        }

        /// <summary>
        /// The create snapshot.
        /// </summary>
        /// <returns>
        /// The RavenQuestionnaire.Core.Documents.CompleteQuestionnaireDocument.
        /// </returns>
        public  CompleteQuestionnaireDocument CreateSnapshot()
        {
            return this.doc;
        }

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.

        
        /// <summary>
        /// The delete propagate group.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationKey">
        /// The propagation key.
        /// </param>
        public void DeletePropagatableGroup(Guid publicKey, Guid propagationKey)
        {
            throw new InvalidOperationException("Is not supported.");
        }

        /// <summary>
        /// The restore from snapshot.
        /// </summary>
        /// <param name="snapshot">
        /// The snapshot.
        /// </param>
        public  void RestoreFromSnapshot(CompleteQuestionnaireDocument snapshot)
        {
            this.doc = snapshot;
        }

        /// <summary>
        /// The set comment.
        /// </summary>
        /// <param name="questionPublickey">
        /// The question public key.
        /// </param>
        /// <param name="comments">
        /// The comments.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propagation public key.
        /// </param>
        public void SetComment(Guid questionPublickey, string comments, Guid? propogationPublicKey, UserLight user)
        {
            this.ApplyEvent(
                new CommentSet
                {
                    User = user,
                    Comments = comments,
                    PropagationPublicKey = propogationPublicKey,
                    QuestionPublickey = questionPublickey
                });
        }
        public void SetFlag(Guid questionPublickey, Guid? propogationPublicKey, bool isFlaged)
        {
            this.ApplyEvent(
                new FlagSet
                {
                    IsFlaged = isFlaged,
                    PropagationPublicKey = propogationPublicKey,
                    QuestionPublickey = questionPublickey
                });
        }
        
        /// <summary>
        /// The set answer.
        /// </summary>
        /// <param name="questionPublicKey">
        /// The question public key.
        /// </param>
        /// <param name="propogationPublicKey">
        /// The propagation public key.
        /// </param>
        /// <param name="completeAnswerValue">
        /// The complete answer value.
        /// </param>
        /// <param name="completeAnswers">
        /// The complete answers.
        /// </param>
        /// <param name="answerDate">
        /// The answer date.
        /// </param>
        public void SetAnswer(Guid questionPublicKey, Guid? propogationPublicKey, string completeAnswerValue, List<Guid> completeAnswers, DateTime answerDate)
        {
            ////performe check before event raising!!
            ICompleteQuestion question = this.doc.GetQuestion(questionPublicKey, propogationPublicKey);
            question.AnswerDate = answerDate;
            ////it's not a great idea to build here answer text
            string answerString;
            if (question.IsValueQuestion())
            {
                answerString = completeAnswerValue;
            }
            else
            {
                var answerList = new List<string>();
                if (completeAnswers == null)
                    throw new InterviewException("optiona are absent");
                foreach (Guid answerGuid in completeAnswers)
                {
                    var answer = question.Answers.FirstOrDefault(q => q.PublicKey == answerGuid);
                    if (answer != null)
                    {
                        answerList.Add(answer.AnswerText);
                    }
                }

                answerString = string.Join(", ", answerList.ToArray());
            }

            question.ThrowDomainExceptionIfAnswerInvalid(completeAnswers, completeAnswerValue);
            ///////////////

            // handle propagation
            var propagatedQuestion = question as AutoPropagateCompleteQuestion;
            if (propagatedQuestion != null)
            {
                this.AddRemovePropagatedGroup(propagatedQuestion, completeAnswerValue);
            }
            
            this.ApplyEvent(
                new AnswerSet
                    {
                        QuestionPublicKey = questionPublicKey, 
                        PropogationPublicKey = propogationPublicKey,
                        AnswerKeys = completeAnswers, 
                        AnswerValue = completeAnswerValue, 
                        AnswerDate = answerDate,
                        Featured = question.Featured, 
                        ////clean up this values
                        QuestionText = question.QuestionText, 
                        AnswerString = answerString
                    });


            // string is used for combination of guid\propagation guid
            // think about using separate structure to hold 2 keys 
            var resultQuestionsStatus = new Dictionary<string, bool?>();
            var resultGroupsStatus = new Dictionary<string, bool?>();

            var collector = ConditionExecuterFactory.GetConditionExecuter(this.doc);

            collector.ExecuteConditionAfterAnswer(question, resultQuestionsStatus, resultGroupsStatus);

            if (resultQuestionsStatus.Count > 0 || resultGroupsStatus.Count > 0)
            {
                this.ApplyEvent(
                    new ConditionalStatusChanged()
                        {
                            CompletedQuestionnaireId = this.EventSourceId,
                            ResultGroupsStatus = resultGroupsStatus,
                            ResultQuestionsStatus = resultQuestionsStatus
                        });
            }
        }

        public void DeleteInterview(Guid deletedBy)
        {
            if (this.doc.Status == SurveyStatus.Unknown || this.doc.Status == SurveyStatus.Unassign ||
                this.doc.Status == SurveyStatus.Initial)
            {
                this.ApplyEvent(
                    new InterviewDeleted()
                    {
                        DeletedBy = deletedBy
                    });
            }
            else
            {
                throw new DomainException(DomainExceptionType.CouldNotDeleteInterview, "Couldn't delete completed interview");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add remove propagated group.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <param name="completeAnswerValue">
        /// The complete answer value.
        /// </param>
        /// <exception cref="ArgumentException">
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        public void AddRemovePropagatedGroup(AutoPropagateCompleteQuestion question, string completeAnswerValue)
        {
            int count;
            //// check is it true for all cases?
            if (string.IsNullOrWhiteSpace(completeAnswerValue))
            {
                count = 0;
            }
            else if (!int.TryParse(completeAnswerValue, out count))
            {
                throw new ArgumentException("Value is not a number");
            }

            if (count < 0)
            {
                throw new ArgumentException("Count can't be bellow zero");
            }

            if (count > question.MaxValue)
            {
                throw new ArgumentException(string.Format("Value can't be greater than {0}.", question.MaxValue));
            }

            var currentAnswer = question.Answer ?? 0;
            
            if (currentAnswer == count)
            {
                return;
            }

            this.HandlePropagation(question, currentAnswer, count);
        }

        /// <summary>
        /// The handle propagation.
        /// </summary>
        /// <param name="autoQuestion">
        /// The auto question.
        /// </param>
        /// <param name="currentAnswer">
        /// The current answer.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// </exception>
        private void HandlePropagation(AutoPropagateCompleteQuestion autoQuestion, int currentAnswer, int count)
        {
            if (currentAnswer < count)
            {
                //// collect all group templates by trigger keys
                var triggers = autoQuestion.Triggers.Distinct().ToList();
                if (triggers.Any())
                {
                    // Create keys for propagation
                    // to search group just once for all keys 
                    // it's faster to search template just ones
                    Guid[] keysPropagate = new Guid[count - currentAnswer];
                    for (int i = 0; i < count - currentAnswer; i++)
                    {
                        keysPropagate[i] = Guid.NewGuid();
                    }

                    //// get the scope of the changes
                    IComposite scopeRoot = this.GetRootForQuestion(autoQuestion);


                    var resultQuestionsStatus = new Dictionary<string, bool?>();
                    var resultGroupsStatus = new Dictionary<string, bool?>();

                    var collector = ConditionExecuterFactory.GetConditionExecuter(this.doc);
                    
                    ////iterate over all triggers for question
                    foreach (var trigger in triggers)
                    {
                        Guid trigger1 = trigger;
                        //// find all templates by trigger
                        var templatesToCreate = scopeRoot.Find<CompleteGroup>(g => g.PublicKey == trigger1 && g.PropagationPublicKey == null).ToList();

                        foreach (var completeGroup in templatesToCreate)
                        {
                            foreach (var guid in keysPropagate)
                            {
                                var parent = completeGroup.GetParent() as CompleteGroup;
                                if (parent == null)
                                {
                                    throw new InvalidOperationException("Incorrect parent-child relationship.");
                                }

                                // temporary old  event is back
                                this.ApplyEvent(
                                    new PropagatableGroupAdded
                                    {
                                        /*CompletedQuestionnaireId = this.doc.PublicKey,*/

                                        PublicKey = trigger,
                                        PropagationKey = guid,
                                        ParentKey = parent.PublicKey,
                                        ParentPropagationKey = parent.PropagationPublicKey,
                                    });

                                /*this.ApplyEvent(new PropagateGroupCreated
                                {
                                    Group = new CompleteGroup(completeGroup, guid),
                                    ParentPublicKey = parent.PublicKey,
                                    ParentPropagationKey = parent.PropagationPublicKey
                                });*/

                                // assuming that group was propagated
                                var item = this.doc.Find<ICompleteGroup>(
                                    g => g.PublicKey == trigger && g.PropagationPublicKey == guid).First();

                                var completeItem = item.GetParent() as ICompleteItem;
                                collector.CollectGroupHierarhicallyStates(item, completeItem != null && completeItem.Enabled, resultGroupsStatus, resultQuestionsStatus);
                            }
                        }
                    }

                    this.ApplyEvent(
                        new ConditionalStatusChanged()
                        {
                            CompletedQuestionnaireId = this.EventSourceId,
                            ResultGroupsStatus = resultGroupsStatus,
                            ResultQuestionsStatus = resultQuestionsStatus
                        });
                }
            }
            else
            {
                //// assuming that all propagations were correct
                //// so we need just get correspondent number of propagation keys

                //// collect all group templates by trigger keys
                var triggers = autoQuestion.Triggers.Distinct().ToList();
                if (triggers.Any())
                {
                    //// get the scope of the changes
                    IComposite scopeRoot = this.GetRootForQuestion(autoQuestion);

                    //// key of first triggered group 
                    var firstKey = triggers.First();

                    //// list of propagation keys created propagation created
                    var propagatedGroups = scopeRoot.Find<CompleteGroup>(g => g.PublicKey == firstKey && g.PropagationPublicKey.HasValue).Select(i => i.PropagationPublicKey).ToArray();

                    if (propagatedGroups.Length != currentAnswer)
                    {
                        throw new InvalidOperationException("Mismatch between structure and answer.");
                    }

                    for (int i = currentAnswer; i > count; i--)
                    {
                        foreach (Guid trigger in triggers)
                        {
                            var lastGroup = scopeRoot.Find<CompleteGroup>(g => g.PublicKey == trigger && g.PropagationPublicKey == propagatedGroups[i - 1]).FirstOrDefault(); 
                            if (lastGroup != null)
                            {
                                var parent = lastGroup.GetParent() as CompleteGroup;
                                if (parent == null)
                                {
                                    throw new InvalidOperationException("Incorrect parent-child relationship.");
                                }

                                this.ApplyEvent(
                                    new PropagatableGroupDeleted
                                    {
                                        PublicKey = lastGroup.PublicKey,
                                        PropagationKey = lastGroup.PropagationPublicKey.Value,
                                        ParentKey = parent.PublicKey,
                                        ParentPropagationKey = parent.PropagationPublicKey
                                    });
                            }
                        }
                    }
                }
            }
        }

        public void CreateNewAssigment(CompleteQuestionnaireDocument source)
        {
            ApplyEvent(new NewAssigmentCreated() { Source = source });
        }

        public void UpdateInterviewMetaInfo(Guid templateId, string title, Guid? responsibleId, Guid statusId, List<FeaturedQuestionMeta> featuredAnswers)
        {
            ApplyEvent(new InterviewMetaInfoUpdated() { FeaturedQuestionsMeta = featuredAnswers, ResponsibleId = responsibleId, StatusId = statusId, TemplateId = templateId, Title = title });
        }

        /// <summary>
        /// The change assignment.
        /// </summary>
        /// <param name="responsible">
        /// The responsible.
        /// </param>
        public void ChangeAssignment(UserLight responsible)
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

        public void AssignInterviewToUser(Guid userId)
        {
            var prevResponsible = this.doc.Responsible;
            this.ApplyEvent(
                new QuestionnaireAssignmentChanged
                    {
                        CompletedQuestionnaireId = this.doc.PublicKey,
                        PreviousResponsible = prevResponsible,
                        Responsible = new UserLight(userId, string.Empty)
                    });
            this.ChangeStatus(SurveyStatus.Initial, new UserLight(userId, string.Empty));
        }

        /// <summary>
        /// The change status.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="responsible">
        /// The responsible.
        /// </param>
        public void ChangeStatus(SurveyStatus status, UserLight responsible)
        {
            var prevStatus = this.doc.Status;

            this.ApplyEvent(
                new QuestionnaireStatusChanged
                    {
                        CompletedQuestionnaireId = this.doc.PublicKey, 
                        Status = status,
                        PreviousStatus = prevStatus,
                        Responsible = responsible
                    });
        }

        protected void OnNewAssigmentCreated(NewAssigmentCreated e)
        {
            this.doc = e.Source;
        }

        protected void OnInterviewMetaInfoUpdated(InterviewMetaInfoUpdated e)
        {
            if (doc == null)
                doc = new CompleteQuestionnaireDocument();
            doc.PublicKey = this.EventSourceId;
            doc.Status = SurveyStatus.GetStatusByIdOrDefault(e.StatusId);
            if (e.ResponsibleId.HasValue)
                doc.Responsible = new UserLight(e.ResponsibleId.Value, "");
            doc.TemplateId = e.TemplateId;
            doc.Title = e.Title;
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
            ICompleteQuestion question = this.doc.GetQuestion(e.QuestionPublicKey, e.PropogationPublicKey);
            if (question == null)
            {
                return; ////is it good or exception is better decision?
            }
            
            question.SetAnswer(e.AnswerKeys, e.AnswerValue);
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
        /// The on new questionnaire created.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnNewQuestionnaireCreated(NewCompleteQuestionnaireCreated e)
        {
            this.doc = e.Questionnaire;
            this.doc.ConnectChildsWithParent();
            ////this.conditionDependencies = ExpressionDependencyBuilder.Build(this.doc);
        }

        /// <summary>
        /// The on propagate group added.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnPropagatableGroupAdded(PropagatableGroupAdded e)
        {
            /*
             *1. Find Question had triggered propagation
             *2. Define the scope of influence of question - only the boundaries of current propagation
             *If this question belongs to propagated group
             *Global questions don't affect through propagation boundary
             *For instance Nested group of 2 level cannot be referred from outside question
             *if 
             */
            //// right now this ivent is not handeled correctly for autopropagate questions
            //// which are laying inside propagated group

            var template = this.doc.Find<CompleteGroup>(g => g.PublicKey == e.PublicKey && g.PropagationPublicKey == null).FirstOrDefault();
            var newGroup = new CompleteGroup(template, e.PropagationKey);
            this.doc.Add(newGroup, e.ParentKey, e.ParentPropagationKey);
        }

        /// <summary>
        /// The on propagate group created.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnConditionalStatusChanged(ConditionalStatusChanged e)
        {
            // to do the serching and set status. 
            foreach (var item in e.ResultGroupsStatus)
            {
                var group =
                    this.doc.Find<ICompleteGroup>(
                        q => CompleteQuestionnaireConditionExecuteCollector.GetGroupHashKey(q) == item.Key).FirstOrDefault();
                if (group != null)
                {
                    group.Enabled = item.Value != false;
                }
            }

            foreach (var item in e.ResultQuestionsStatus)
            {
                var question = this.doc.GetQuestionByKey(item.Key);
                if (question != null)
                {
                    question.Question.Enabled = item.Value != false;
                }
            }
        }

        /// <summary>
        /// The on propagate group created.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnPropagateGroupCreated(PropagateGroupCreated e)
        {
            this.doc.Add(e.Group, e.ParentKey, e.ParentPropagationKey);
        }

        /// <summary>
        /// The get root for question.
        /// </summary>
        /// <param name="question">
        /// The question.
        /// </param>
        /// <returns>
        /// The <see cref="IComposite"/>.
        /// </returns>
        private IComposite GetRootForQuestion(ICompleteQuestion question)
        {
            //// question doesn't belog to propagate group
            //// so the root is the whole document
            if (question.PropagationPublicKey == null)
            {
                return this.doc;
            }

            //// Navigate throught hierarchy to find scope bounds 
            IComposite parent = question.GetParent();

            while (true)
            {
                var item = parent as CompleteGroup;
                if (item == null)
                {
                    break;
                }
                
                if (item.PropagationPublicKey != question.PropagationPublicKey || item.GetParent() == null)
                {
                    break;
                }

                parent = item.GetParent();
            }

            return parent;
        }

        /// <summary>
        /// The on propagatable group deleted.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <exception cref="CompositeException">
        /// </exception>
        protected void OnPropagatableGroupDeleted(PropagatableGroupDeleted e)
        {
            this.doc.Remove(e.PublicKey, e.PropagationKey, e.ParentKey, e.ParentPropagationKey);
        }

        /// <summary>
        /// The on set comment command.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnSetCommentCommand(CommentSet e)
        {
            ICompleteQuestion question = this.doc.GetQuestion(e.QuestionPublickey, e.PropagationPublicKey);
            if (question == null)
            {
                return;
            }

            question.SetComments(e.Comments, DateTime.Now, e.User);
        }

        /// <summary>
        /// The on set comment command.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void OnSetFlagCommand(FlagSet e)
        {
            ICompleteQuestion question = this.doc.GetQuestion(e.QuestionPublickey, e.PropagationPublicKey);
            if (question == null)
            {
                return;
            }

            question.IsFlaged = e.IsFlaged;
        }

        protected void OnInterviewDeleted(InterviewDeleted e)
        {
            this.doc.IsDeleted = true;
            this.doc.DeletedBy = e.DeletedBy;
        }

        #endregion
    }
}