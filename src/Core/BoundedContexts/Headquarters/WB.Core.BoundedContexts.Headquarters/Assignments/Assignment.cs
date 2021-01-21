using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class Assignment : IReadSideRepositoryEntity
    {
        public Assignment()
        {
            this.Answers = new List<InterviewAnswer>();
            this.IdentifyingData = new List<IdentifyingAnswer>();
            this.InterviewSummaries = new HashSet<InterviewSummary>();
            this.ProtectedVariables = new List<string>();
            this.GpsAnswers = new HashSet<AssignmentGps>();
        }

        internal Assignment(
            Guid publicKey,
            int id,
            QuestionnaireIdentity questionnaireId,
            Guid responsibleId,
            int? quantity,
            bool audioRecording,
            string email,
            string password,
            bool? webMode, 
            string comments) : this()
        {
            this.PublicKey = publicKey;
            this.Id = id;
            this.ResponsibleId = responsibleId;
            this.Quantity = quantity;
            this.QuestionnaireId = questionnaireId;
            this.AudioRecording = audioRecording;
            this.Email = email;
            this.Password = password;
            this.WebMode = webMode;
            this.Comments = comments;
        }

        public virtual Guid PublicKey { get; set; }

        public virtual int Id { get; set; }

        public virtual Guid ResponsibleId { get; set; }

        public virtual ReadonlyUser Responsible { get; protected set; }

        public virtual int? Quantity { get; set; }

        public virtual bool Archived { get; set; }
        
        public virtual bool QuantityCanBeChanged => !this.Archived && this.WebMode != true;


        public virtual DateTime CreatedAtUtc { get; set; }

        public virtual DateTime UpdatedAtUtc { get; set; }

        public virtual DateTime? ReceivedByTabletAtUtc { get; set; }

        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }

        public virtual bool AudioRecording { get; set; }

        public virtual string Email { get; set; }
        public virtual string Password { get; set; }
        public virtual bool? WebMode { get; set; }

        public virtual IList<IdentifyingAnswer> IdentifyingData { get; set; }

        public virtual IList<InterviewAnswer> Answers { get; set; }

        public virtual List<string> ProtectedVariables { get; set; }

        public virtual QuestionnaireBrowseItem Questionnaire { get; protected set; }

        public virtual ISet<InterviewSummary> InterviewSummaries { get; protected set; }

        public virtual int? InterviewsNeeded => this.Quantity.HasValue
            ? this.Quantity - this.InterviewSummaries.Count
            : null;

        public virtual bool IsCompleted => this.InterviewsNeeded <= 0;

        public virtual string Comments { get; set; }

        public virtual bool InPrivateWebMode()
        {
            return (WebMode == null || WebMode == true) && Quantity == 1;
        }
        
        public virtual ISet<AssignmentGps> GpsAnswers { get; protected set; }

        public static List<InterviewAnswer> GetAnswersFromInterview(IStatefulInterview interview, IQuestionnaire questionnaire)
        {
            var answers = new List<InterviewAnswer>();

            var prefilledQuestions = questionnaire.GetPrefilledQuestions();
            foreach (var prefilledQuestion in prefilledQuestions)
            {
                var questionIdentity = new Identity(prefilledQuestion, RosterVector.Empty);

                var question = interview.GetQuestion(questionIdentity);
                if (question.IsAnswered())
                {
                    answers.Add(
                        new InterviewAnswer
                        {
                            Identity = questionIdentity,
                            Answer = question.InterviewQuestion.Answer
                        });
                }
            }

            return answers;
        }

        public virtual void SetComments(string comments)
        {
            this.Comments = comments;
            this.UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
