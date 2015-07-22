using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using it = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CascadingComboboxTests
{
    internal class AnswerOptionsForCascadingQuestionsDenormalizerTestContext
    {
        protected static AnswerOptionsForCascadingQuestionsDenormalizer CreateAnswerOptionsForCascadingQuestionsDenormalizer(
            IReadSideRepositoryWriter<InterviewViewModel> interviewStorage = null, ILogger logger = null)
        {
            return new AnswerOptionsForCascadingQuestionsDenormalizer(
                interviewStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewViewModel>>(),
                logger ?? Mock.Of<ILogger>());
        }

        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid? eventSourceId = null)
           where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == (eventSourceId ?? Guid.Parse("55555555555555555555555555555555")));
        }

        protected static InterviewViewModel CreateInterviewViewModel(Guid interviewId)
        {
            return new InterviewViewModel(interviewId, Mock.Of<QuestionnaireDocument>(), new QuestionnaireRosterStructure());
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument();
        }
    }
}
