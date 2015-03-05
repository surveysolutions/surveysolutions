using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewViewModel : MvxViewModel
    {
        private IList<InterviewEntity> prefilledQuestions;
        public IList<InterviewEntity> PrefilledQuestions
        {
            get { return prefilledQuestions; }
            set
            {
                prefilledQuestions = value;
                RaisePropertyChanged(() => PrefilledQuestions);
            }
        }

        private ObservableCollection<InterviewGroup> chapters;
        public ObservableCollection<InterviewGroup> Chapters
        {
            get { return chapters; }
            set
            {
                chapters = value;
                RaisePropertyChanged(() => Chapters);
            }
        }

        private ObservableCollection<InterviewEntity> groupsAndQuestions;
        public ObservableCollection<InterviewEntity> GroupsAndQuestions
        {
            get { return groupsAndQuestions; }
            set
            {
                groupsAndQuestions = value;
                RaisePropertyChanged(() => GroupsAndQuestions);
            }
        }

        public async void Init(Guid interviewId)
        {
            await Task.Run(() => LoadInterview());
        }

        private void LoadInterview()
        {
            
        }
    }
}
