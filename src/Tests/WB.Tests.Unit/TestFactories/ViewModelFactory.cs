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

namespace WB.Tests.Unit.TestFactories
{
        internal class ViewModelFactory
        {
            public ValidityViewModel ValidityViewModel(ILiteEventRegistry eventRegistry = null,
                                                            IStatefulInterviewRepository interviewRepository = null,
                                                            IQuestionnaire questionnaire = null,
                                                            Identity entityIdentity = null)
            {
                var result = new ValidityViewModel(eventRegistry ?? Create.Service.LiteEventRegistry(),
                    interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                    Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                    Stub.MvxMainThreadDispatcher());

                return result;
            }

            public StaticTextStateViewModel StaticTextStateViewModel(IStatefulInterviewRepository interviewRepository = null,
                ILiteEventRegistry eventRegistry = null)
            {
                return new StaticTextStateViewModel(Create.ViewModel.EnablementViewModel(interviewRepository, eventRegistry),
                    Create.ViewModel.ValidityViewModel(interviewRepository: interviewRepository));
            }

            private EnablementViewModel EnablementViewModel(IStatefulInterviewRepository interviewRepository = null, 
                ILiteEventRegistry eventRegistry = null)
            {
                return new EnablementViewModel(interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(), 
                    eventRegistry ?? Create.Service.LiteEventRegistry());
            }
        }
}