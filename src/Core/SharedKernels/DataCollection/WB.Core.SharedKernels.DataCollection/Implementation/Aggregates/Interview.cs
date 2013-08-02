using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Domain;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention
    {
        #region State

        private Guid questionnaireId;
        private long questionnaireVersion;

        private void Apply(InterviewCreated @event)
        {
            this.questionnaireId = @event.QuestionnaireId;
            this.questionnaireVersion = @event.QuestionnaireVersion;
        }

        #endregion

        #region Dependencies

        /// <summary>
        /// Repository which allows to get questionnaire.
        /// Should be used only in command handlers.
        /// And never in event handlers!!
        /// </summary>
        private IQuestionnaireRepository QuestionnaireRepository
        {
            get { return ServiceLocator.Current.GetInstance<IQuestionnaireRepository>(); }
        }

        #endregion

        /// <remarks>Is used to restore aggregate from event stream.</remarks>
        public Interview() {}

        public Interview(Guid questionnaireId, Guid userId)
        {
            IQuestionnaire questionnaire = GetQuestionnaireOrThrowInterviewException(questionnaireId);

            this.ApplyEvent(new InterviewCreated(userId, questionnaireId, questionnaire.Version));
        }


        private IQuestionnaire GetQuestionnaireOrThrowInterviewException(Guid questionnaireId)
        {
            IQuestionnaire questionnaire = this.QuestionnaireRepository.GetQuestionnaire(questionnaireId);

            if (questionnaire == null)
                throw new InterviewException(string.Format("Questionnaire with id '{0}' is not found.", questionnaireId));

            return questionnaire;
        }
    }
}
