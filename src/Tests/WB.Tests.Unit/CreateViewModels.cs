using Moq;
using MvvmCross.Platform;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit
{
    internal static partial class Create
    {
        internal static class ViewModels
        {
            public static ValidityViewModel ValidityViewModel(ILiteEventRegistry eventRegistry = null,
                                                            IStatefulInterviewRepository interviewRepository = null,
                                                            IQuestionnaire questionnaire = null,
                                                            Identity entityIdentity = null)
            {
                var result = new ValidityViewModel(eventRegistry ?? Create.LiteEventRegistry(),
                    interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                    Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                    Stub.MvxMainThreadDispatcher());

                return result;
            }

            public static StaticTextStateViewModel StaticTextStateViewModel(IStatefulInterviewRepository interviewRepository = null,
                ILiteEventRegistry eventRegistry = null)
            {
                return new StaticTextStateViewModel(Create.ViewModels.EnablementViewModel(interviewRepository, eventRegistry),
                    Create.ViewModels.ValidityViewModel(interviewRepository: interviewRepository));
            }

            private static EnablementViewModel EnablementViewModel(IStatefulInterviewRepository interviewRepository = null, 
                ILiteEventRegistry eventRegistry = null)
            {
                return new EnablementViewModel(interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(), 
                    eventRegistry ?? Create.LiteEventRegistry());
            }
        }

        public static SynchronizationViewModel SynchronizationViewModel()
        {
            return new SynchronizationViewModel();
        }
    }
}