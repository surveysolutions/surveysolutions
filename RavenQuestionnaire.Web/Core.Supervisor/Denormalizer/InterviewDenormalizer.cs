using Core.Supervisor.Views.Interview;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Denormalizer
{
    using System.Linq;

    using Main.Core.Events.Questionnaire.Completed;
    using Ncqrs.Eventing.ServiceModel.Bus;

    public class InterviewDenormalizer : UserBaseDenormalizer,
        IEventHandler<NewCompleteQuestionnaireCreated>,
                                                               IEventHandler<AnswerSet>,
                                                               IEventHandler<QuestionnaireStatusChanged>,
                                                               IEventHandler<QuestionnaireAssignmentChanged>,
                                                               IEventHandler<InterviewDeleted>
    {
        private readonly IReadSideRepositoryWriter<InterviewItem> interviews;

        public InterviewDenormalizer(IReadSideRepositoryWriter<InterviewItem> interviews,
                                     IReadSideRepositoryWriter<UserDocument> users) 
            :base(users)
        {
            this.interviews = interviews;
        }

        #region Public Methods and Operators

        public void Handle(IPublishedEvent<NewCompleteQuestionnaireCreated> evnt)
        {
            var interview = new InterviewItem()
            {
                InterviewId = evnt.Payload.Questionnaire.PublicKey,
                TemplateId = evnt.Payload.Questionnaire.TemplateId,
                Title = evnt.Payload.Questionnaire.Title,
                LastEntryDate = evnt.EventTimeStamp,
                Status = new SurveyStatusLight() { Id = evnt.Payload.Questionnaire.Status.PublicId, Name = evnt.Payload.Questionnaire.Status.Name},
                FeaturedQuestions =
                    evnt.Payload.Questionnaire.GetFeaturedQuestions()
                        .Select(
                            x =>
                                new InterviewFeaturedQuestion()
                                {
                                    Id = x.PublicKey,
                                    Question = x.QuestionText,
                                    Answer = x.GetAnswerString()
                                })
            };

            this.interviews.Store(interview, interview.InterviewId);
        }

        public void Handle(IPublishedEvent<AnswerSet> evnt)
        {
            if (evnt.Payload.Featured)
            {
                var item = this.interviews.GetById(evnt.EventSourceId);
                if (item == null)
                {
                    return;
                }

                item.LastEntryDate = evnt.EventTimeStamp;
                var currentFeatured =
                    item.FeaturedQuestions.FirstOrDefault(q => q.Id == evnt.Payload.QuestionPublicKey);

                if (currentFeatured != null)
                {
                    currentFeatured.Answer = evnt.Payload.AnswerString;
                }
                

                this.interviews.Store(item, item.InterviewId);
            }
        }
        
        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var item = this.interviews.GetById(evnt.EventSourceId);

            item.Status = new SurveyStatusLight() {Id = evnt.Payload.Status.PublicId, Name = evnt.Payload.Status.Name};
            item.LastEntryDate = evnt.EventTimeStamp;
            item.IsDeleted = false;
            this.interviews.Store(item, item.InterviewId);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var responsible = this.FillResponsiblesName(evnt.Payload.Responsible);

            var item = this.interviews.GetById(evnt.EventSourceId);

            var user = this.users.GetById(responsible.Id);

            item.ResponsibleSupervisorId =
                user.Supervisor == null ? user.PublicKey : user.Supervisor.Id;
            item.Responsible = responsible;

            item.LastEntryDate = evnt.EventTimeStamp;

            this.interviews.Store(item, item.InterviewId);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            var item = this.interviews.GetById(evnt.EventSourceId);
            item.IsDeleted = true;
            this.interviews.Store(item, item.InterviewId);
        }

        #endregion
    }
}