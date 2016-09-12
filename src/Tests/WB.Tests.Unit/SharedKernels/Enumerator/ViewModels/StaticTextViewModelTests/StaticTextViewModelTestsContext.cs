using Machine.Specifications;
using Moq;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    [Subject(typeof(StaticTextViewModel))]
    internal class StaticTextViewModelTestsContext
    {
        public static StaticTextViewModel CreateViewModel(IQuestionnaireStorage questionnaireRepository = null, 
            IStatefulInterviewRepository interviewRepository = null, 
            ILiteEventRegistry registry = null,
            IRosterTitleSubstitutionService rosterTitleSubstitutionService = null,
            AttachmentViewModel attachmentViewModel = null,
            StaticTextStateViewModel questionState = null,
            SubstitutionViewModel substitutionViewModel = null)
        {
            var statefulInterviewRepository = interviewRepository ?? Mock.Of<IStatefulInterviewRepository>();
            var plainQuestionnaireRepository = questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>();

            var liteEventRegistry = registry ?? Create.Service.LiteEventRegistry();

            return new StaticTextViewModel(
                questionnaireRepository: plainQuestionnaireRepository,
                interviewRepository: statefulInterviewRepository,
                questionState: questionState ??
                               new StaticTextStateViewModel(
                                   new EnablementViewModel(statefulInterviewRepository, liteEventRegistry, plainQuestionnaireRepository),
                                   new ValidityViewModel(liteEventRegistry, statefulInterviewRepository,
                                       plainQuestionnaireRepository, Mock.Of<IMvxMainThreadDispatcher>(), Create.ViewModel.ErrorMessagesViewModel())),
                attachmentViewModel: attachmentViewModel ?? new AttachmentViewModel(plainQuestionnaireRepository, statefulInterviewRepository, Mock.Of<IAttachmentContentStorage>()),
                dynamicTextViewModel: Create.ViewModel.DynamicTextViewModel(
                    eventRegistry: liteEventRegistry,
                    interviewRepository: statefulInterviewRepository,
                    questionnaireRepository: plainQuestionnaireRepository,
                    rosterTitleSubstitutionService: rosterTitleSubstitutionService));
        }
    }
}