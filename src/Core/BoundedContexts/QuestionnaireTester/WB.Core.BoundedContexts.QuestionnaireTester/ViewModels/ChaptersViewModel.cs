using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class ChapterViewModel : MvxNotifyPropertyChanged
    {
        private Identity chapterIdentity;

        private string title;
        public string Title
        {
            get { return this.title; }
            set { this.title = value; this.RaisePropertyChanged(); }
        }

        public ChapterViewModel(Guid id, decimal[] rosterVector, string chapterTitle)
        {
            chapterIdentity = new Identity(id, rosterVector);
            Title = chapterTitle;
        }
    }

    public class ChaptersViewModel : MvxNotifyPropertyChanged
    {
        private NavigationState navigationState;

        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

        public IList<ChapterViewModel> Chapters { get; set; }

        public ChaptersViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository, 
            IStatefullInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, string questionnaireId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException("navigationState");
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");

            this.navigationState = navigationState;
            this.navigationState.OnGroupChanged += navigationState_OnGroupChanged;

            var questionnaire = questionnaireRepository.GetById(questionnaireId);
            Chapters = questionnaire
                .GroupsHierarchy
                .Select(x => new ChapterViewModel(x.Id, new decimal[0], x.Title))
                .ToList();
        }

        void navigationState_OnGroupChanged(Identity newGroupIdentity)
        {

        }
    }
}