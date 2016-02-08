using Cirrious.CrossCore;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
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
                    Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire));
                result.Init("interviewid", entityIdentity, Create.NavigationState(interviewRepository));

                Mvx.RegisterSingleton(Stub.MvxMainThreadDispatcher());

                return result;
            }
        }
    }
}