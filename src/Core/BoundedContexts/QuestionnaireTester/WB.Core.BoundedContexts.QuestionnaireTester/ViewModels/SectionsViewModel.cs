using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.Messenger;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class SectionsViewModel : MvxNotifyPropertyChanged
    {
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IMvxMessenger messenger;

        public IList<SectionViewModel> Sections { get; set; }

        public SectionsViewModel(
            IMvxMessenger messenger,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.messenger = messenger;
        }

        public void Init(string questionnaireId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;

            var questionnaire = questionnaireRepository.GetById(questionnaireId);
            this.Sections = questionnaire
                .GroupsHierarchy
                .Select(x => new SectionViewModel(this) { SectionIdentity = new Identity(x.Id, new decimal[0]), Title = x.Title })
                .ToList();
        }

        internal async Task NavigateToSection(SectionViewModel item)
        {
            messenger.Publish(new SectionChangeMessage(this));

            if (item.IsSelected)
            {
                return;
            }
            
            await this.navigationState.NavigateTo(item.SectionIdentity);
        }

        void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {
            var sectionToBeSelected = Sections.FirstOrDefault(x => x.SectionIdentity.Equals(newGroupIdentity));
            if (sectionToBeSelected == null)
            {
                return;
            }

            this.Sections.Where(x => x.IsSelected).ForEach(x => x.IsSelected = false);
            sectionToBeSelected.IsSelected = true;
        }
    }
}