using System;
using System.Threading.Tasks;
using MvvmCross.Base;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class FilteredSingleOptionQuestionViewModel : BaseComboboxQuestionViewModel
    {
        private readonly QuestionAttachmentViewModel attachmentViewModel;

        public FilteredSingleOptionQuestionViewModel(
            IStatefulInterviewRepository interviewRepository,
            IViewModelEventRegistry eventRegistry,
            FilteredOptionsViewModel filteredOptionsViewModel,
            IPrincipal principal,
            QuestionStateViewModel<SingleOptionQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IInterviewViewModelFactory viewModelFactory,
            AttachmentViewModel attachmentViewModel) :
            base(principal: principal, questionStateViewModel: questionStateViewModel, answering: answering,
                instructionViewModel: instructionViewModel, interviewRepository: interviewRepository, 
                eventRegistry: eventRegistry, filteredOptionsViewModel, viewModelFactory)
        {
            this.attachmentViewModel = new QuestionAttachmentViewModel(attachmentViewModel);
        }

        public override void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            base.Init(interviewId, entityIdentity, navigationState);

            this.filteredOptionsViewModel.OptionsChanged += FilteredOptionsViewModelOnOptionsChanged;

            this.comboboxCollection.Add(attachmentViewModel);
        }

        protected override void ChangeAttachment(int? optionValue)
        {
            string attachmentName = null;
            if (optionValue.HasValue)
            {
                var interview = this.interviewRepository.GetOrThrow(this.interviewId);
                attachmentName = interview.GetAttachmentForEntityOption(Identity, optionValue.Value, null);
            }

            this.attachmentViewModel.Attachment.InitAsStatic(interviewId, attachmentName);
        }

        private async Task FilteredOptionsViewModelOnOptionsChanged(object sender, EventArgs eventArgs)
        {
            await comboboxViewModel.UpdateFilter(comboboxViewModel.FilterText, true);
        }

        public override void Dispose()
        {
            base.Dispose();
            this.filteredOptionsViewModel.OptionsChanged -= FilteredOptionsViewModelOnOptionsChanged;
            this.filteredOptionsViewModel.Dispose();
        }
        
        public override IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(comboboxCollection);
                result.Add(this.optionsBottomBorderViewModel);

                return result;
            }
        }
    }
}
